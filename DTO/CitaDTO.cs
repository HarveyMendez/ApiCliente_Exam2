namespace ApiCliente.DTO
{
    public class CitaDTO
    {
        public string cedula_paciente {  get; set; }

        public string cedula_medico { get; set; }

        public string nombre_hospital {  get; set; }

        public DateTime fecha_hora { get; set; }
    }
}
