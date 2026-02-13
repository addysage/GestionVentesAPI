using Microsoft.AspNetCore.SignalR;
using GestionVentesAPI.Models;

namespace GestionVentesAPI.Hubs
{
    /// <summary>
    /// Hub SignalR pour la synchronisation temps réel des ventes
    /// </summary>
    public class SalesHub : Hub
    {
        private static readonly Dictionary<string, string> _adminConnections = new();
        private static readonly Dictionary<string, string> _vendeurConnections = new();

        /// <summary>
        /// ✅ Connexion d'un client
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"✅ Nouvelle connexion: {connectionId}");

            await Clients.Caller.SendAsync("Connected", new
            {
                connectionId,
                timestamp = DateTime.UtcNow,
                message = "✅ Connecté au serveur SignalR"
            });

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// ✅ Déconnexion d'un client
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            // Supprimer des listes d'admin/vendeur
            _adminConnections.Remove(connectionId);
            _vendeurConnections.Remove(connectionId);

            Console.WriteLine($"❌ Déconnexion: {connectionId}");

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// ✅ Enregistrer un appareil comme admin
        /// </summary>
        public async Task RegisterAsAdmin(string deviceName)
        {
            var connectionId = Context.ConnectionId;
            _adminConnections[connectionId] = deviceName;

            Console.WriteLine($"👨‍💼 Admin enregistré: {deviceName} ({connectionId})");

            await Clients.Caller.SendAsync("AdminRegistered", new
            {
                deviceName,
                connectionId,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// ✅ Enregistrer un vendeur
        /// </summary>
        public async Task RegisterAsVendeur(string vendeurNom)
        {
            var connectionId = Context.ConnectionId;
            _vendeurConnections[connectionId] = vendeurNom;

            Console.WriteLine($"🛒 Vendeur enregistré: {vendeurNom} ({connectionId})");

            await Clients.Caller.SendAsync("VendeurRegistered", new
            {
                vendeurNom,
                connectionId,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// ✅ BROADCASTER UNE VENTE À TOUS LES ADMINS
        /// </summary>
        public async Task BroadcastVente(HistoriqueVente vente)
        {
            try
            {
                Console.WriteLine($"📤 Broadcast vente: {vente.ProduitNom} - {vente.MontantTotal:N0} FC par {vente.VendeurNom}");

                // ✅ Envoyer à tous les admins connectés
                if (_adminConnections.Any())
                {
                    foreach (var adminConnection in _adminConnections.Keys)
                    {
                        await Clients.Client(adminConnection).SendAsync("ReceiveVente", vente);
                    }
                    Console.WriteLine($"✅ Vente envoyée à {_adminConnections.Count} admin(s)");
                }
                else
                {
                    Console.WriteLine("⚠️ Aucun admin connecté");
                }

                // ✅ Envoyer également à tous les autres clients (broadcast général)
                await Clients.All.SendAsync("ReceiveVente", vente);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur BroadcastVente: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ✅ Notifier les admins d'un message spécifique
        /// </summary>
        public async Task NotifyAdmin(string message)
        {
            try
            {
                Console.WriteLine($"🔔 Notification admin: {message}");

                foreach (var adminConnection in _adminConnections.Keys)
                {
                    await Clients.Client(adminConnection).SendAsync("ReceiveNotification", message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur NotifyAdmin: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ Mettre à jour le stock
        /// </summary>
        public async Task UpdateStock(int produitId, string nomProduit, int nouvelleQuantite)
        {
            try
            {
                Console.WriteLine($"📦 Mise à jour stock: {nomProduit} -> {nouvelleQuantite}");

                await Clients.All.SendAsync("ReceiveStockUpdate", produitId, nomProduit, nouvelleQuantite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur UpdateStock: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ Obtenir le nombre de clients connectés
        /// </summary>
        public async Task<object> GetConnectedClients()
        {
            var result = new
            {
                admins = _adminConnections.Count,
                vendeurs = _vendeurConnections.Count,
                total = _adminConnections.Count + _vendeurConnections.Count,
                adminDevices = _adminConnections.Values.ToList(),
                vendeurNames = _vendeurConnections.Values.ToList()
            };

            await Clients.Caller.SendAsync("ConnectedClientsInfo", result);
            return result;
        }

        /// <summary>
        /// ✅ Ping pour tester la connexion
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", new
            {
                timestamp = DateTime.UtcNow,
                message = "🏓 Pong"
            });
        }
    }
}