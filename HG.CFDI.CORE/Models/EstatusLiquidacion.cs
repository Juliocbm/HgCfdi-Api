namespace HG.CFDI.CORE.Models
{
    public enum EstatusLiquidacion : byte
    {
        Pendiente = 0,
        EnProceso = 1,
        ErrorTransitorio = 4,
        RequiereRevision = 2,
        Timbrado = 3,
        Migrada = 5
    }
}
