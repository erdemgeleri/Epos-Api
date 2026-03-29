using Microsoft.AspNetCore.Identity;
class U { }
var h = new PasswordHasher<U>();
var pwd = args.Length > 0 ? args[0] : "PosDemo2026!";
Console.WriteLine(h.HashPassword(new U(), pwd));
