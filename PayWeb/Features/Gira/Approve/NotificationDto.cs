using System.Collections.Generic;

namespace CRM.Features.Gira.Approve
{
    public class NotificationDto
    {
        public List<string> Users { get; set; } = new();        // a quién: códigos de usuario destino
        public string Title { get; set; } = string.Empty;       // título de la notificación
        public string? Body { get; set; }                       // cuerpo / mensaje
        public string? Category { get; set; }                   // ej. "general", "solicitud_compra"
        public Dictionary<string, string>? Data { get; set; }   // metadatos opcionales (ids, deep-link, etc.)
    }
}
