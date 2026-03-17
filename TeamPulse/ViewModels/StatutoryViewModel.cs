namespace TeamPulse.Models
{
    public class StatutoryViewModel
    {
        public int MonthNo { get; set; }
        public string MonthName { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalPF { get; set; }
        public decimal TotalESI { get; set; }
        public decimal TotalPT { get; set; }
        public decimal TotalTDS { get; set; }

        public decimal TotalLiability => TotalPF + TotalESI + TotalPT + TotalTDS;
    }
}