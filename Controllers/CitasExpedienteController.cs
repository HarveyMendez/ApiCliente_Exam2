using ApiCliente.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace ApiCliente.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitasExpedienteController : ControllerBase
    {
        private readonly Utils _utils;

        public CitasExpedienteController(Utils utils)
        {
            _utils = utils;
        }

        //[HttpPost("/EncriptarTexto")]
        //public string EncriptarString(string stringAconvertir)
        //{
        //    //encriptar
        //    string encryptedConnectionString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(stringAconvertir));
        //    return encryptedConnectionString;
        //}

        //[HttpPost("/DesencriptarText")]
        //public string DecryptConnectionString(string encryptedConnectionString)
        //{

        //    byte[] decodedBytes = Convert.FromBase64String(encryptedConnectionString);
        //    string decryptedConnectionString = System.Text.Encoding.UTF8.GetString(decodedBytes);

        //    return decryptedConnectionString;
        //}

        [HttpPost("/RegitrarPaciente")]
        public IActionResult RegistrarPaciente(PacienteDTO infoPaciente)
        {
            using (SqlConnection conn = _utils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_registrar_paciente", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@nombre", infoPaciente.Nombre);
                cmd.Parameters.AddWithValue("@apellidos", infoPaciente.Apellidos);
                cmd.Parameters.AddWithValue("@cedula", infoPaciente.Cedula);
                cmd.Parameters.AddWithValue("@telefono", infoPaciente.Telefono);
                cmd.Parameters.AddWithValue("@correo_electronico", infoPaciente.Correo_electronico);

                SqlDataReader dr = cmd.ExecuteReader();
                conn.Close();
            }
            return Ok();
        }

        [HttpPost("/RegitrarCita")]
        public IActionResult RegistrarCita(CitaDTO infoCita)
        {
            using (SqlConnection conn = _utils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_registrar_cita", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@cedula_paciente", infoCita.cedula_paciente);
                cmd.Parameters.AddWithValue("@cedula_medico", infoCita.cedula_medico);
                cmd.Parameters.AddWithValue("@nombre_hospital", infoCita.nombre_hospital);
                string fechaHoraString = infoCita.fecha_hora.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                cmd.Parameters.AddWithValue("@fecha_hora", fechaHoraString);

                SqlDataReader dr = cmd.ExecuteReader();

                string mensajeResultado = "";
                if (dr.Read())
                {
                    mensajeResultado = dr["Mensaje"].ToString();
                }

                conn.Close();

                return Ok(mensajeResultado);
            }
        }

        [HttpGet("/ConsultarExpedientePaciente")]
        public IActionResult ConsultarExpedientePaciente(string cedula_paciente)
        {
            List<ExpedienteDTO> expedientes = new List<ExpedienteDTO>();
            using (SqlConnection conn = _utils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_consultar_expediente_cedula", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@cedula_paciente", cedula_paciente);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    expedientes.Add(new ExpedienteDTO
                    {
                        fecha_creacion = (DateTime)dr["fecha_creacion"],
                        altura = (decimal)dr["altura"],
                        peso = (decimal)dr["peso"],
                        padecimientos = (string)dr["padecimientos"]
                    });
                }
                conn.Close();
            }

            return Ok(expedientes);
        }

        [HttpPost("/RegistrarExpediente")]
        public IActionResult RegistrarExpediente(decimal altura, decimal peso, string padecimientos, string cedula_paciente)
        {
            using (SqlConnection conn = _utils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_registrar_expediente", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@cedula_paciente", cedula_paciente);
                cmd.Parameters.AddWithValue("@peso_paciente", peso);
                cmd.Parameters.AddWithValue("@altura_paciente", altura);
                cmd.Parameters.AddWithValue("@padecimientos_paciente", padecimientos);

                SqlDataReader dr = cmd.ExecuteReader();

                string mensajeResultado = "";
                if (dr.Read())
                {
                    mensajeResultado = dr["Mensaje"].ToString();
                }

                conn.Close();

                return Ok(mensajeResultado);
            }
        }

        [HttpPost("/EmitirRecetaMedica")]
        public IActionResult EmitirRecetaMedica(RecetaDTO infoReceta)
        {
            using (SqlConnection conn = _utils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_emitir_receta", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id_cita", infoReceta.id_cita);
                cmd.Parameters.AddWithValue("@nombre_hospital", infoReceta.nombre_hospital);
                cmd.Parameters.AddWithValue("@medicamentos", infoReceta.medicamentos);
                

                SqlDataReader dr = cmd.ExecuteReader();

                string mensajeResultado = "";
                if (dr.Read())
                {
                    mensajeResultado = dr["Mensaje"].ToString();
                }

                conn.Close();

                return Ok(mensajeResultado);
            }
        }

        [HttpGet("/GestionarNotificaciones")]
        public IActionResult GestionarNotificaciones()
        {
            List<EmailDTO> notificaciones = new List<EmailDTO>();
            using (SqlConnection conn = _utils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_obtener_notificaciones_pendientes", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    notificaciones.Add(new EmailDTO
                    {
                        Id = (int)dr["id_notificacion"],
                        Subject = (string)dr["asunto"],
                        Body = (string)dr["cuerpo"],
                        AddressTo = (string)dr["correo"]
                    });
                }
                conn.Close();
            }
            EnviarCorreos(notificaciones);

            return Ok();
        }

        private void MarcarNotificacionEnviada(int idNotificacion)
        {
            using (SqlConnection conn = _utils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_marcar_notificacion_enviada", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdNotificacion", idNotificacion);

                SqlDataReader dr = cmd.ExecuteReader();
                conn.Close();
            }
        }

        private async void EnviarCorreos(List<EmailDTO> notificaciones)
        {
            foreach (EmailDTO notificacion in notificaciones)
            {
                using HttpResponseMessage response = await _utils.GetAPIHost().PostAsJsonAsync(_utils.GetEmailAPI(), notificacion);
                if (response.IsSuccessStatusCode)
                {
                    MarcarNotificacionEnviada(notificacion.Id);
                }
            }
        }
    }
}
