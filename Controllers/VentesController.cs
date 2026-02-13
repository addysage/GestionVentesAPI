using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GestionVentesAPI.Hubs;
using GestionVentesAPI.Models;

namespace GestionVentesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentesController : ControllerBase
    {
        private readonly IHubContext<SalesHub> _hubContext;
        private readonly ILogger<VentesController> _logger;

        public VentesController(IHubContext<SalesHub> hubContext, ILogger<VentesController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// ✅ Endpoint REST pour enregistrer une vente
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<HistoriqueVente>>> PostVente([FromBody] HistoriqueVente vente)
        {
            try
            {
                _logger.LogInformation("📤 Vente reçue: {ProduitNom} - {MontantTotal} FC", vente.ProduitNom, vente.MontantTotal);

                // ✅ Broadcaster la vente via SignalR
                await _hubContext.Clients.All.SendAsync("ReceiveVente", vente);

                return Ok(new ApiResponse<HistoriqueVente>
                {
                    Success = true,
                    Message = "✅ Vente enregistrée et diffusée avec succès",
                    Data = vente
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur enregistrement vente");
                return StatusCode(500, new ApiResponse<HistoriqueVente>
                {
                    Success = false,
                    Message = $"❌ Erreur: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ✅ Obtenir le statut du serveur
        /// </summary>
        [HttpGet("status")]
        public ActionResult<object> GetStatus()
        {
            return Ok(new
            {
                status = "🟢 En ligne",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                signalRHub = "/salesHub"
            });
        }

        /// <summary>
        /// ✅ Test de connexion
        /// </summary>
        [HttpGet("test")]
        public ActionResult<object> Test()
        {
            return Ok(new
            {
                message = "✅ API GestionVentes fonctionne correctement",
                timestamp = DateTime.UtcNow
            });
        }
    }
}