using System.Text.Json;

namespace GateFall.Extensions
{
    public static class SessionExtensions
    {
        // Nesneyi JSON yapıp kaydeder
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // JSON'u okuyup nesneye çevirir
        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}