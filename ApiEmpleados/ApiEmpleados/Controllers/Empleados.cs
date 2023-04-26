using ApiEmpleados.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using Dapper;
using System.Transactions;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace ApiEmpleados.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Empleados : ControllerBase
    {
         
        private readonly string _configuration;

        public Empleados(IConfiguration configuration )
        {
          _configuration= configuration.GetConnectionString("DefaultConnection");

        }
       
       

        public static string EncriptarContrasena(string contrasena)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(contrasena);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Función para comparar una contraseña encriptada con una contraseña no encriptada
        public static bool CompararContrasenas(string contrasenaEncriptada, string contrasenaNoEncriptada)
        {
            string contrasenaEncriptadaUsuario = EncriptarContrasena(contrasenaNoEncriptada);
            return contrasenaEncriptada == contrasenaEncriptadaUsuario;
        }





        [HttpPost("NuevoEmpleado")]

        public async Task<IActionResult> NuevoEmpleado([FromBody] Empleado empleado)
        {

            using (var conn = new SqlConnection(_configuration))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var hijosDetalle = new DataTable();
                        hijosDetalle.Columns.Add("nombres", typeof(string));
                        hijosDetalle.Columns.Add("apellidos", typeof(string));
                        hijosDetalle.Columns.Add("sexo", typeof(string));
                        hijosDetalle.Columns.Add("fecha_nacimiento", typeof(DateTime));
                        hijosDetalle.Columns.Add("edad", typeof(int));

                        foreach (var hijos in empleado.Hijos)
                        {
                            hijosDetalle.Rows.Add(hijos.Nombres, hijos.Apellidos, hijos.Sexo, hijos.Fecha_Nacimiento, hijos.Edad);
                        }

                        var parameters = new DynamicParameters();
                        parameters.Add("@documento_identidad", empleado.Documento_Identidad);
                        parameters.Add("@nombres", empleado.Nombres);
                        parameters.Add("@apellidos", empleado.Apellidos);
                        parameters.Add("@nacionalidad", empleado.Nacionalidad);
                        parameters.Add("@telefono", empleado.Telefono);
                        parameters.Add("@sexo", empleado.Sexo);
                        parameters.Add("@fecha_nacimiento", empleado.Fecha_Nacimiento);
                        parameters.Add("@salario", empleado.Salario);
                        parameters.Add("@fecha_ingreso", empleado.Fecha_Ingreso);
                        parameters.Add("@puestoID", empleado.PuestoID);
                        parameters.Add("@hijos", hijosDetalle.AsTableValuedParameter("TipoHijos"));
                        await conn.ExecuteAsync(
                            "NuevoEmpleado",
                            parameters,
                            transaction,
                            commandType: CommandType.StoredProcedure);
                        transaction.Commit();
                        return Ok();

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                    }
                }

            }

        }

        [HttpPut("{id}",Name ="ActualizarEmpleado")]

        public async Task<IActionResult> ActualizarEmpleado(int id, [FromBody] Empleado empleado)
        {
            

                using (var conn=new SqlConnection(_configuration))
                {
                    await conn.OpenAsync();
                    using (var transaction = conn.BeginTransaction())
                    {
                    try
                    {
                        var hijosDetalle = new DataTable();
                        hijosDetalle.Columns.Add("nombres", typeof(string));
                        hijosDetalle.Columns.Add("apellidos", typeof(string));
                        hijosDetalle.Columns.Add("sexo", typeof(string));
                        hijosDetalle.Columns.Add("fecha_nacimiento", typeof(DateTime));
                        hijosDetalle.Columns.Add("edad", typeof(int));

                        foreach (var hijos in empleado.Hijos)
                        {
                            hijosDetalle.Rows.Add(hijos.Nombres, hijos.Apellidos, hijos.Sexo, hijos.Fecha_Nacimiento, hijos.Edad);
                        }

                        var parameters = new DynamicParameters();
                        parameters.Add("@empleadoID", id);
                        parameters.Add("@documento_identidad", empleado.Documento_Identidad);
                        parameters.Add("@nombres", empleado.Nombres);
                        parameters.Add("@apellidos", empleado.Apellidos);
                        parameters.Add("@nacionalidad", empleado.Nacionalidad);
                        parameters.Add("@telefono", empleado.Telefono);
                        parameters.Add("@sexo", empleado.Sexo);
                        parameters.Add("@fecha_nacimiento", empleado.Fecha_Nacimiento);
                        parameters.Add("@salario", empleado.Salario);
                        parameters.Add("@fecha_ingreso", empleado.Fecha_Ingreso);
                        parameters.Add("@puestoID", empleado.PuestoID);
                        parameters.Add("@hijos", hijosDetalle.AsTableValuedParameter("TipoHijos"));
                        await conn.ExecuteAsync(
                           "ActualizarEmpleado",
                           parameters,
                           transaction,
                           commandType: CommandType.StoredProcedure);
                        transaction.Commit();
                        return Ok();

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

                    }

                    }

                }

            
           
        }

        [HttpGet("EmpleadosHijos")]

        public async Task<IActionResult> EmpleadosConMasHijos()
        {
            using (var conn= new SqlConnection(_configuration))
            {
                await conn.OpenAsync();
                var parameters = new DynamicParameters();
                var results = await conn.QueryAsync<Empleado>("empleadosConMasHijos", parameters, commandType: CommandType.StoredProcedure);
                return Ok(results);
            }
        }

        [HttpPost("NuevoUsuario")]

        public async Task<IActionResult> NuevoUsuario([FromBody] Usuario usuario)
        {

            using (var conn = new SqlConnection(_configuration))
            {
                await conn.OpenAsync();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string contrasenaEncriptada = EncriptarContrasena(usuario.contrasena);                    
                        var parameters = new DynamicParameters();
                        parameters.Add("@correo", usuario.correo);
                        parameters.Add("@contrasena", contrasenaEncriptada);
                        parameters.Add("@empleadoID", usuario.empleadoID);
                        parameters.Add("@tipoUsuarioID", usuario.tipoUsuarioID);                                            
                        await conn.ExecuteAsync(
                            "NuevoUsuario",
                            parameters,
                            transaction,
                            commandType: CommandType.StoredProcedure);
                        transaction.Commit();
                        return Ok();

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); 
                        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                    }
                }

            }

        }
  

        [HttpPost("InicioSesion")]

        public async Task<IActionResult> InicioSesion([FromBody] InicioSesion usuario)
        {

            string consulta = "select contrasena,usuarioID from usuarios where correo=@correo";

            using (SqlConnection conn=new SqlConnection(_configuration))
            {
                SqlCommand command = new SqlCommand(consulta, conn);
                command.Parameters.AddWithValue("@correo", usuario.correo);
                try
                {
                    await conn.OpenAsync();
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string contrasenaObtenida=reader.GetString(0).Trim();
                        int usuarioID = reader.GetInt32(1);


                        if (CompararContrasenas(contrasenaObtenida, usuario.contrasena))
                        {
                            var claims = new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, usuarioID.ToString()),
                                new Claim(ClaimTypes.Name, usuario.correo),
                            };

                            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("my_secret_key50505050"));
                            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                            var token = new JwtSecurityToken(
                                issuer: "my_issuer",
                                audience: "my_audience",
                                claims: claims,
                                expires: DateTime.Now.AddDays(1),
                                signingCredentials: creds);

                            return Ok(new
                            {
                                token = new JwtSecurityTokenHandler().WriteToken(token)
                            });
                        }
                       
                        reader.Close();

                    }
                    return Ok();
                }
                catch (Exception ex)
                {

                    return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
                }
            }
        }








    }
}

