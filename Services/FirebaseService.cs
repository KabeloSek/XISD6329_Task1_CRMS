using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace XISD6329_Task1_CRMS.Services
{
    // Thrown with a message that is safe to show to the user.
    public class FirebaseException : Exception
    {
        public FirebaseException(string message) : base(message) { }
    }

    // A signed-in user (student or cleaner). Same fields as users/{uid} in the database.
    public class UserProfile
    {
        public string Uid { get; set; } = "";
        public string Role { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Room { get; set; } = "";
        public string ExternalBookingId { get; set; } = "";
    }

    // A booking, as stored under bookings/{id}.
    public class Booking
    {
        public string Id { get; set; } = "";
        public string StudentId { get; set; } = "";
        public string RoomNumber { get; set; } = "";
        public string BookingDate { get; set; } = "";   // yyyy-MM-dd
        public string RoomType { get; set; } = "";
        public string TimeSlot { get; set; } = "";
        public string CleaningType { get; set; } = "";
        public string SpecialInstructions { get; set; } = "";
        public string Status { get; set; } = "";
        public string? CleanerId { get; set; }
        public string? CleanerName { get; set; }
        public string RequestedBy { get; set; } = "";
        public long CreatedAt { get; set; }
        public string? EnteredPasskey { get; set; }
    }

    // A notification, as stored under notifications/{studentId}/{id}.
    public class AppNotification
    {
        public string Message { get; set; } = "";
        public string Type { get; set; } = "";
        public string? CleanerName { get; set; }
        public string? Passkey { get; set; }
        public string? BookingDate { get; set; }
        public long CreatedAt { get; set; }
    }

    /// <summary>
    /// The ONLY place the web app talks to Firebase. It uses the same Firebase project and the same
    /// Realtime Database layout as the Android app:
    ///
    ///   users/{uid}                     role, name, email, room, externalBookingId
    ///   externalIds/{externalId}        uid
    ///   rooms/{room}                    true
    ///   bookings/{bookingId}            studentId, requestedBy, roomNumber, bookingDate, roomType, timeSlot,
    ///                                   cleaningType, specialInstructions, status, cleanerId, cleanerName,
    ///                                   createdAt, enteredPasskey
    ///   bookingSecrets/{bookingId}      studentId, passkey
    ///   notifications/{studentId}/{id}  message, type, bookingId, cleanerName, passkey, bookingDate, createdAt
    ///   problemReports/{id}             studentId, issue, room, photoAttached, createdAt
    ///
    /// Every database call is made AS the logged-in user (their Firebase ID token), so the same database
    /// rules that protect the Android app protect the website too.
    /// The tokens live in the ASP.NET session.
    /// </summary>
    public class FirebaseService
    {
        private readonly HttpClient http;
        private readonly string apiKey;
        private readonly string dbUrl;
        private readonly IHttpContextAccessor accessor;

        public FirebaseService(HttpClient http, IConfiguration config, IHttpContextAccessor accessor)
        {
            this.http = http;
            this.accessor = accessor;
            apiKey = config["Firebase:ApiKey"] ?? "";
            dbUrl = (config["Firebase:DatabaseUrl"] ?? "").TrimEnd('/');
        }

        private ISession Session => accessor.HttpContext!.Session;

        // =============================================================== session helpers

        public string? CurrentUid => Session.GetString("Uid");
        public string? CurrentRole => Session.GetString("Role");
        public string CurrentName => Session.GetString("Name") ?? "";
        public string CurrentEmail => Session.GetString("Email") ?? "";
        public string CurrentRoom => Session.GetString("Room") ?? "";

        public bool IsStudent => CurrentUid != null && CurrentRole == "student";
        public bool IsCleaner => CurrentUid != null && CurrentRole == "cleaner";

        public void Logout() => Session.Clear();

        private void SaveTokens(JsonNode json, string uid)
        {
            // sign-in answers use idToken/refreshToken/expiresIn, the refresh answer uses id_token/...
            string idToken = (json["idToken"] ?? json["id_token"])!.ToString();
            string refresh = (json["refreshToken"] ?? json["refresh_token"])!.ToString();
            int seconds = int.Parse((json["expiresIn"] ?? json["expires_in"])!.ToString());

            Session.SetString("Uid", uid);
            Session.SetString("IdToken", idToken);
            Session.SetString("RefreshToken", refresh);
            Session.SetString("TokenExpires", DateTime.UtcNow.AddSeconds(seconds - 60).Ticks.ToString());
        }

        // Firebase tokens last one hour, so quietly swap in a new one when it has run out.
        private async Task<string> ValidToken()
        {
            string? token = Session.GetString("IdToken");
            string? refresh = Session.GetString("RefreshToken");
            string? expires = Session.GetString("TokenExpires");
            if (token == null || refresh == null || expires == null)
                throw new FirebaseException("You are not signed in.");

            if (DateTime.UtcNow.Ticks < long.Parse(expires)) return token;

            var response = await http.PostAsync("https://securetoken.googleapis.com/v1/token?key=" + apiKey,
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refresh
                }));
            if (!response.IsSuccessStatusCode)
            {
                Session.Clear();
                throw new FirebaseException("Your session expired. Please log in again.");
            }
            JsonNode json = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
            SaveTokens(json, CurrentUid!);
            return Session.GetString("IdToken")!;
        }

        // =============================================================== Firebase Authentication (REST)

        private async Task<JsonNode> AuthCall(string endpoint, object body)
        {
            var response = await http.PostAsJsonAsync(
                "https://identitytoolkit.googleapis.com/v1/" + endpoint + "?key=" + apiKey, body);
            JsonNode? json = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            if (!response.IsSuccessStatusCode)
                throw new FirebaseException(FriendlyAuthError(json?["error"]?["message"]?.ToString()));
            return json!;
        }

        private static string FriendlyAuthError(string? code)
        {
            code = code ?? "";
            if (code.StartsWith("WEAK_PASSWORD")) return "Password must be at least 6 characters.";
            if (code.StartsWith("EMAIL_EXISTS")) return "That email is already registered.";
            if (code.StartsWith("TOO_MANY_ATTEMPTS")) return "Too many attempts. Please wait a bit and try again.";
            if (code.StartsWith("INVALID_EMAIL")) return "That email address is not valid.";
            if (code is "EMAIL_NOT_FOUND" or "INVALID_PASSWORD" or "INVALID_LOGIN_CREDENTIALS" or "USER_DISABLED")
                return "Invalid email or password.";
            return "Something went wrong (" + code + ").";
        }

        // =============================================================== Realtime Database (REST)

        // method: GET / PUT / PATCH / DELETE. path: e.g. "bookings/abc". query: extra url parameters.
        private async Task<JsonNode?> Db(HttpMethod method, string path, object? body = null,
                                         string query = "", string? token = null)
        {
            token ??= await ValidToken();
            string url = dbUrl + "/" + path + ".json?auth=" + Uri.EscapeDataString(token) + query;

            var request = new HttpRequestMessage(method, url);
            if (body != null) request.Content = JsonContent.Create(body);

            var response = await http.SendAsync(request);
            string text = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new FirebaseException("Permission denied.");
            if (!response.IsSuccessStatusCode)
                throw new FirebaseException("Database error: " + text);

            return string.IsNullOrWhiteSpace(text) || text == "null" ? null : JsonNode.Parse(text);
        }

        // Several writes in ONE request, so they all succeed or all fail (same as the Android updateChildren).
        // A null value deletes that path.
        private Task<JsonNode?> MultiUpdate(Dictionary<string, object?> updates, string? token = null)
            => Db(HttpMethod.Patch, "", updates, "", token);

        private static string Q(string text) => Uri.EscapeDataString("\"" + text + "\"");

        // =============================================================== rooms

        // Room numbers already claimed by a registered student (readable before signing in).
        public async Task<HashSet<string>> TakenRooms()
        {
            var response = await http.GetAsync(dbUrl + "/rooms.json?shallow=true");
            if (!response.IsSuccessStatusCode) return new HashSet<string>();
            JsonNode? json = JsonNode.Parse(await response.Content.ReadAsStringAsync());
            var rooms = new HashSet<string>();
            if (json is JsonObject obj)
                foreach (var pair in obj) rooms.Add(pair.Key);
            return rooms;
        }

        // =============================================================== register / login

        public async Task RegisterStudent(string room, string name, string email, string password)
        {
            if ((await TakenRooms()).Contains(room))
                throw new FirebaseException("Room " + room + " is already taken. Please choose another room.");

            JsonNode account = await AuthCall("accounts:signUp",
                new { email = email.Trim(), password, returnSecureToken = true });
            string uid = account["localId"]!.ToString();
            string token = account["idToken"]!.ToString();

            string externalId = "EB" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            try
            {
                // profile + external id + room claim, all in one write
                await MultiUpdate(new Dictionary<string, object?>
                {
                    ["users/" + uid] = new
                    {
                        role = "student",
                        name = name.Trim(),
                        email = email.Trim(),
                        room,
                        externalBookingId = externalId
                    },
                    ["externalIds/" + externalId] = uid,
                    ["rooms/" + room] = true
                }, token);
            }
            catch (FirebaseException)
            {
                await DeleteAccount(token);   // don't leave a half-created account behind
                throw new FirebaseException("Could not save your profile. That room may have just been taken.");
            }
        }

        public async Task RegisterCleaner(string name, string email, string password)
        {
            JsonNode account = await AuthCall("accounts:signUp",
                new { email = email.Trim(), password, returnSecureToken = true });
            string uid = account["localId"]!.ToString();
            string token = account["idToken"]!.ToString();

            try
            {
                await Db(HttpMethod.Put, "users/" + uid,
                    new { role = "cleaner", name = name.Trim(), email = email.Trim() }, "", token);
            }
            catch (FirebaseException)
            {
                await DeleteAccount(token);
                throw new FirebaseException("Could not save your profile. Please try again.");
            }
        }

        private async Task DeleteAccount(string idToken)
        {
            try { await AuthCall("accounts:delete", new { idToken }); } catch { }
        }

        // Logs in and keeps the user in the session. expectedRole is "student" or "cleaner".
        public async Task<UserProfile> Login(string email, string password, string expectedRole)
        {
            JsonNode json = await AuthCall("accounts:signInWithPassword",
                new { email = email.Trim(), password, returnSecureToken = true });
            string uid = json["localId"]!.ToString();
            SaveTokens(json, uid);

            UserProfile? profile = await GetProfile(uid);
            if (profile == null || profile.Role != expectedRole)
            {
                Session.Clear();
                throw new FirebaseException(expectedRole == "student"
                    ? "This is not a student account. Use the cleaner login."
                    : "This is not a cleaner account. Use the student login.");
            }

            Session.SetString("Role", profile.Role);
            Session.SetString("Name", profile.Name);
            Session.SetString("Email", profile.Email);
            Session.SetString("Room", profile.Room);
            return profile;
        }

        public async Task<UserProfile?> GetProfile(string uid)
        {
            JsonNode? node = await Db(HttpMethod.Get, "users/" + uid);
            if (node == null) return null;
            return new UserProfile
            {
                Uid = uid,
                Role = Str(node, "role") ?? "",
                Name = Str(node, "name") ?? "",
                Email = Str(node, "email") ?? "",
                Room = Str(node, "room") ?? "",
                ExternalBookingId = Str(node, "externalBookingId") ?? ""
            };
        }

        // Profile page: change email and/or password. Firebase needs the current password for both.
        // An email change is confirmed through a link Firebase emails to the NEW address.
        // Returns a message to show the user.
        public async Task<string> UpdateCredentials(string currentPassword, string newEmail, string newPassword)
        {
            string oldEmail = CurrentEmail;
            bool emailChanged = !string.IsNullOrWhiteSpace(newEmail) &&
                                !newEmail.Trim().Equals(oldEmail, StringComparison.OrdinalIgnoreCase);
            bool passwordChanged = !string.IsNullOrWhiteSpace(newPassword);
            if (!emailChanged && !passwordChanged) return "Nothing to update.";

            if (string.IsNullOrWhiteSpace(currentPassword))
                throw new FirebaseException("Enter your current password to make changes.");

            // re-check the current password (Firebase calls this re-authentication)
            JsonNode fresh;
            try
            {
                fresh = await AuthCall("accounts:signInWithPassword",
                    new { email = oldEmail, password = currentPassword, returnSecureToken = true });
            }
            catch (FirebaseException)
            {
                throw new FirebaseException("Your current password is incorrect.");
            }
            SaveTokens(fresh, CurrentUid!);
            string idToken = fresh["idToken"]!.ToString();

            var messages = new List<string>();

            if (emailChanged)
            {
                await AuthCall("accounts:sendOobCode",
                    new { requestType = "VERIFY_AND_CHANGE_EMAIL", idToken, newEmail = newEmail.Trim() });
                messages.Add("A verification link was sent to " + newEmail.Trim() +
                             ". Your email changes once you confirm it.");
            }
            if (passwordChanged)
            {
                JsonNode updated = await AuthCall("accounts:update",
                    new { idToken, password = newPassword, returnSecureToken = true });
                SaveTokens(updated, CurrentUid!);   // the password change gives us new tokens
                messages.Add("Password updated.");
            }
            return string.Join(" ", messages);
        }

        // =============================================================== bookings

        // Returns the uid of the student that owns this External Booking ID, or null.
        public async Task<string?> FindStudentByExternalId(string externalId)
        {
            JsonNode? node = await Db(HttpMethod.Get, "externalIds/" + Uri.EscapeDataString(externalId.Trim().ToUpper()));
            return node?.ToString();
        }

        // Pending booking + secret passkey + "Requested" notification, all in one write.
        public async Task CreateBooking(string targetStudentId, bool forSomeoneElse, string room, DateTime date,
                                        string roomType, string timeSlot, string cleaningType, string? instructions)
        {
            string requester = CurrentUid ?? throw new FirebaseException("You are not signed in.");
            string bookingId = NewId();
            string notificationId = NewId();
            string passkey = Random.Shared.Next(100000, 1000000).ToString();
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string dateText = date.ToString("yyyy-MM-dd");

            string message = forSomeoneElse
                ? "A cleaning request has been submitted on your behalf. Give this passkey to your cleaner once the job is done."
                : "Your cleaning request has been submitted. Give this passkey to your cleaner once the job is done.";

            await MultiUpdate(new Dictionary<string, object?>
            {
                ["bookings/" + bookingId] = BookingMap(new Booking
                {
                    StudentId = targetStudentId,
                    RequestedBy = requester,
                    RoomNumber = room,
                    BookingDate = dateText,
                    RoomType = roomType,
                    TimeSlot = timeSlot,
                    CleaningType = cleaningType,
                    SpecialInstructions = (instructions ?? "").Trim(),
                    Status = "Pending",
                    CreatedAt = now
                }),
                ["bookingSecrets/" + bookingId] = new { studentId = targetStudentId, passkey },
                ["notifications/" + targetStudentId + "/" + notificationId] =
                    NotificationMap(bookingId, message, "Requested", null, passkey, dateText, now)
            });
        }

        // Cleaner accepts: Pending -> In Progress. Returns false if someone else got it first.
        public async Task<bool> AcceptBooking(string bookingId)
        {
            Booking? booking = await GetBooking(bookingId);
            if (booking == null || booking.Status != "Pending") return false;

            booking.Status = "In Progress";
            booking.CleanerId = CurrentUid;
            booking.CleanerName = CurrentName;

            try
            {
                await MultiUpdate(new Dictionary<string, object?>
                {
                    ["bookings/" + bookingId] = BookingMap(booking),
                    ["notifications/" + booking.StudentId + "/" + NewId()] = NotificationMap(
                        bookingId, CurrentName + " has accepted your cleaning request.", "Accepted",
                        CurrentName, null, booking.BookingDate, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                });
                return true;
            }
            catch (FirebaseException e) when (e.Message == "Permission denied.")
            {
                return false;
            }
        }

        // Cleaner completes by typing the passkey. The database rules compare it with the secret one,
        // so a wrong passkey is refused by the database (returns false).
        public async Task<bool> CompleteBooking(string bookingId, string enteredPasskey)
        {
            Booking? booking = await GetBooking(bookingId);
            if (booking == null || booking.Status != "In Progress") return false;

            booking.Status = "Completed";
            booking.EnteredPasskey = enteredPasskey.Trim();

            try
            {
                await MultiUpdate(new Dictionary<string, object?>
                {
                    ["bookings/" + bookingId] = BookingMap(booking),
                    ["notifications/" + booking.StudentId + "/" + NewId()] = NotificationMap(
                        bookingId, "Your cleaning has been completed by " + CurrentName + ".", "Completed",
                        CurrentName, null, booking.BookingDate, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                });
                return true;
            }
            catch (FirebaseException e) when (e.Message == "Permission denied.")
            {
                return false;
            }
        }

        public async Task SubmitProblemReport(string issue, string room, bool photoAttached)
        {
            await Db(HttpMethod.Post, "problemReports", new
            {
                studentId = CurrentUid,
                issue = issue.Trim(),
                room,
                photoAttached,
                createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        // =============================================================== reading lists

        private async Task<Booking?> GetBooking(string bookingId)
        {
            JsonNode? node = await Db(HttpMethod.Get, "bookings/" + Uri.EscapeDataString(bookingId));
            return node == null ? null : ToBooking(bookingId, node);
        }

        private async Task<List<Booking>> BookingsWhere(string field, string value)
        {
            JsonNode? node = await Db(HttpMethod.Get, "bookings", null,
                "&orderBy=" + Q(field) + "&equalTo=" + Q(value));
            var list = new List<Booking>();
            if (node is JsonObject obj)
                foreach (var pair in obj) list.Add(ToBooking(pair.Key, pair.Value!));
            return list;
        }

        public async Task<List<Booking>> StudentBookings()
            => (await BookingsWhere("studentId", CurrentUid!)).OrderByDescending(b => b.BookingDate).ToList();

        public async Task<List<Booking>> OpenRequests()
            => (await BookingsWhere("status", "Pending")).OrderBy(b => b.BookingDate).ToList();

        public async Task<List<Booking>> CleanerBookings()
            => (await BookingsWhere("cleanerId", CurrentUid!)).OrderBy(b => b.BookingDate).ToList();

        public async Task<List<AppNotification>> Notifications()
        {
            JsonNode? node = await Db(HttpMethod.Get, "notifications/" + CurrentUid);
            var list = new List<AppNotification>();
            if (node is JsonObject obj)
            {
                foreach (var pair in obj)
                {
                    JsonNode n = pair.Value!;
                    list.Add(new AppNotification
                    {
                        Message = Str(n, "message") ?? "",
                        Type = Str(n, "type") ?? "",
                        CleanerName = Str(n, "cleanerName"),
                        Passkey = Str(n, "passkey"),
                        BookingDate = Str(n, "bookingDate"),
                        CreatedAt = Long(n, "createdAt")
                    });
                }
            }
            return list.OrderByDescending(n => n.CreatedAt).ToList();
        }

        // =============================================================== small helpers

        private static string NewId() => Guid.NewGuid().ToString("N");

        private static string? Str(JsonNode node, string name) => node[name]?.ToString();

        private static long Long(JsonNode node, string name)
            => long.TryParse(node[name]?.ToString(), out long value) ? value : 0;

        private static Booking ToBooking(string id, JsonNode n) => new Booking
        {
            Id = id,
            StudentId = Str(n, "studentId") ?? "",
            RoomNumber = Str(n, "roomNumber") ?? "",
            BookingDate = Str(n, "bookingDate") ?? "",
            RoomType = Str(n, "roomType") ?? "",
            TimeSlot = Str(n, "timeSlot") ?? "",
            CleaningType = Str(n, "cleaningType") ?? "",
            SpecialInstructions = Str(n, "specialInstructions") ?? "",
            Status = Str(n, "status") ?? "",
            CleanerId = Str(n, "cleanerId"),
            CleanerName = Str(n, "cleanerName"),
            RequestedBy = Str(n, "requestedBy") ?? "",
            CreatedAt = Long(n, "createdAt"),
            EnteredPasskey = Str(n, "enteredPasskey")
        };

        // Field names must match the Android app exactly.
        private static Dictionary<string, object?> BookingMap(Booking b) => new Dictionary<string, object?>
        {
            ["studentId"] = b.StudentId,
            ["requestedBy"] = b.RequestedBy,
            ["roomNumber"] = b.RoomNumber,
            ["bookingDate"] = b.BookingDate,
            ["roomType"] = b.RoomType,
            ["timeSlot"] = b.TimeSlot,
            ["cleaningType"] = b.CleaningType,
            ["specialInstructions"] = b.SpecialInstructions,
            ["status"] = b.Status,
            ["cleanerId"] = b.CleanerId,
            ["cleanerName"] = b.CleanerName,
            ["createdAt"] = b.CreatedAt,
            ["enteredPasskey"] = b.EnteredPasskey
        };

        private static Dictionary<string, object?> NotificationMap(string bookingId, string message, string type,
            string? cleanerName, string? passkey, string? bookingDate, long createdAt)
            => new Dictionary<string, object?>
            {
                ["bookingId"] = bookingId,
                ["message"] = message,
                ["type"] = type,
                ["cleanerName"] = cleanerName,
                ["passkey"] = passkey,
                ["bookingDate"] = bookingDate,
                ["createdAt"] = createdAt
            };
    }
}
