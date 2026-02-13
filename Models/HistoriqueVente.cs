namespace GestionVentesAPI.Models
{
    /// <summary>
    /// Modèle de vente pour la synchronisation
    /// </summary>
    public class HistoriqueVente
    {
        public int Id { get; set; }
        public int VendeurId { get; set; }
        public string VendeurNom { get; set; } = string.Empty;
        public int ProduitId { get; set; }
        public string ProduitNom { get; set; } = string.Empty;
        public int QuantiteVendue { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal MontantTotal { get; set; }
        public DateTime DateVente { get; set; }

        public string DateVenteFormatee => DateVente.ToString("dd/MM/yyyy HH:mm");
        public string MontantTotalFormate => $"{MontantTotal:F2} FC";
    }
}