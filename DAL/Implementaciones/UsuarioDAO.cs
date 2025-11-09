using DAL.Interfaces;
using DAL.Utilidades;
using Dapper;
using ENTITY.Usuarios;
using ENTITY.Utilidades;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Implementaciones
{
    public class UsuarioDAO : IUsuarioDAO
    {
        private readonly string _connectionString;

        public UsuarioDAO(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ========================================
        // 1. LOGIN
        // ========================================
        public async Task<Response<LoginResponseDTO>> Login(LoginRequestDTO loginRequest)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_usuarios.login_usuario";
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros de entrada
                        command.Parameters.Add("p_correo", OracleDbType.Varchar2).Value = loginRequest.Correo;
                        command.Parameters.Add("p_contrasena", OracleDbType.Varchar2).Value = loginRequest.Contrasena;

                        // Parámetro de salida (cursor)
                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        // Leer el cursor
                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            if (await reader.ReadAsync())
                            {
                                var usuario = new LoginResponseDTO
                                {
                                    IdUsuario = reader.GetInt32(reader.GetOrdinal("ID_USUARIO")),
                                    NombreCompleto = $"{reader.GetString(reader.GetOrdinal("PRIMER_NOMBRE"))} {reader.GetString(reader.GetOrdinal("APELLIDO_PATERNO"))}",
                                    IdRol = reader.GetInt32(reader.GetOrdinal("ID_ROL"))
                                };

                                Console.WriteLine($"Usuario logueado: {usuario.IdUsuario} - {usuario.NombreCompleto} - Rol: {usuario.IdRol}");

                                return Response<LoginResponseDTO>.Done("Login exitoso", usuario);
                            }
                            else
                            {
                                return Response<LoginResponseDTO>.Fail("Credenciales inválidas");
                            }
                        }
                    }
                }
            }
            catch (OracleException ex) when (ex.Number == 20001)
            {
                return Response<LoginResponseDTO>.Fail("Credenciales incorrectas o usuario inactivo");
            }
            catch (OracleException ex)
            {
                return Response<LoginResponseDTO>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<LoginResponseDTO>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        // ========================================
        // 2. CREAR USUARIO
        // ========================================
        public async Task<Response<int>> CrearUsuario(UsuarioDTO usuario)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_usuarios.crear_usuario";
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros de entrada
                        command.Parameters.Add("p_id_direccion", OracleDbType.Int32).Value =
                            usuario.DireccionId.HasValue ? (object)usuario.DireccionId.Value : DBNull.Value;
                        command.Parameters.Add("p_cedula", OracleDbType.Varchar2).Value = usuario.Cedula;
                        command.Parameters.Add("p_primer_nombre", OracleDbType.Varchar2).Value = usuario.PrimerNombre;
                        command.Parameters.Add("p_segundo_nombre", OracleDbType.Varchar2).Value =
                            usuario.SegundoNombre ?? (object)DBNull.Value;
                        command.Parameters.Add("p_apellido_paterno", OracleDbType.Varchar2).Value = usuario.ApellidoPaterno;
                        command.Parameters.Add("p_apellido_materno", OracleDbType.Varchar2).Value =
                            usuario.ApellidoMaterno ?? (object)DBNull.Value;
                        command.Parameters.Add("p_telefono_principal", OracleDbType.Varchar2).Value = usuario.TelefonoPrincipal;
                        command.Parameters.Add("p_telefono_secundario", OracleDbType.Varchar2).Value =
                            usuario.TelefonoSecundario ?? (object)DBNull.Value;
                        command.Parameters.Add("p_correo", OracleDbType.Varchar2).Value = usuario.Correo;
                        command.Parameters.Add("p_contrasena", OracleDbType.Varchar2).Value = usuario.Contrasena;

                        // Parámetro de salida (ID generado)
                        var idParam = new OracleParameter("p_id_generado", OracleDbType.Int32, ParameterDirection.Output);
                        command.Parameters.Add(idParam);

                        await command.ExecuteNonQueryAsync();

                        int idGenerado = ((OracleDecimal)idParam.Value).ToInt32();

                        return Response<int>.Done("Usuario creado exitosamente", idGenerado);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20002"))
                {
                    return Response<int>.Fail("El correo o la cédula ya existen");
                }
                return Response<int>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<int>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        // ========================================
        // 3. ACTUALIZAR USUARIO
        // ========================================
        public async Task<Response<bool>> ActualizarUsuario(UsuarioDTO usuario)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_usuarios.actualizar_usuario";
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros
                        command.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = usuario.Id;
                        command.Parameters.Add("p_id_direccion", OracleDbType.Int32).Value =
                            usuario.DireccionId.HasValue ? (object)usuario.DireccionId.Value : DBNull.Value;
                        command.Parameters.Add("p_cedula", OracleDbType.Varchar2).Value = usuario.Cedula;
                        command.Parameters.Add("p_primer_nombre", OracleDbType.Varchar2).Value = usuario.PrimerNombre;
                        command.Parameters.Add("p_segundo_nombre", OracleDbType.Varchar2).Value =
                            usuario.SegundoNombre ?? (object)DBNull.Value;
                        command.Parameters.Add("p_apellido_paterno", OracleDbType.Varchar2).Value = usuario.ApellidoPaterno;
                        command.Parameters.Add("p_apellido_materno", OracleDbType.Varchar2).Value =
                            usuario.ApellidoMaterno ?? (object)DBNull.Value;
                        command.Parameters.Add("p_telefono_principal", OracleDbType.Varchar2).Value = usuario.TelefonoPrincipal;
                        command.Parameters.Add("p_telefono_secundario", OracleDbType.Varchar2).Value =
                            usuario.TelefonoSecundario ?? (object)DBNull.Value;
                        command.Parameters.Add("p_correo", OracleDbType.Varchar2).Value = usuario.Correo;
                        command.Parameters.Add("p_contrasena", OracleDbType.Varchar2).Value = usuario.Contrasena;

                        await command.ExecuteNonQueryAsync();

                        return Response<bool>.Done("Usuario actualizado exitosamente", true);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20004"))
                {
                    return Response<bool>.Fail("No se encontró el usuario a actualizar");
                }
                if (ex.Message.Contains("ORA-20005"))
                {
                    return Response<bool>.Fail("Correo o cédula duplicada");
                }
                return Response<bool>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error inesperado: {ex.Message}");
            }
        }

        // ========================================
        // 4. OBTENER USUARIOS
        // ========================================
        public async Task<Response<UsuarioDTO>> ObtenerUsuarios()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_usuarios.listar_usuarios_activos";
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetro de salida (cursor)
                        var cursorParam = new OracleParameter("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
                        command.Parameters.Add(cursorParam);

                        await command.ExecuteNonQueryAsync();

                        var usuarios = new List<UsuarioDTO>();

                        // Leer el cursor
                        using (var reader = ((OracleRefCursor)cursorParam.Value).GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                var usuario = new UsuarioDTO
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ID_USUARIO")),
                                    PrimerNombre = reader.GetString(reader.GetOrdinal("PRIMER_NOMBRE")),
                                    ApellidoPaterno = reader.GetString(reader.GetOrdinal("APELLIDO_PATERNO")),
                                    Correo = reader.GetString(reader.GetOrdinal("CORREO")),
                                    TelefonoPrincipal = reader.GetString(reader.GetOrdinal("TELEFONO_PRINCIPAL")),
                                    FechaRegistro = reader.GetDateTime(reader.GetOrdinal("FECHA_REGISTRO"))
                                };

                                usuarios.Add(usuario);
                            }
                        }

                        return Response<UsuarioDTO>.Done("Usuarios obtenidos exitosamente", list: usuarios);
                    }
                }
            }
            catch (Exception ex)
            {
                return Response<UsuarioDTO>.Fail($"Error al obtener usuarios: {ex.Message}");
            }
        }

        // ========================================
        // 5. ELIMINAR USUARIO
        // ========================================
        public async Task<Response<bool>> EliminarUsuario(int idUsuario)
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "pkg_usuarios.eliminar_usuario";
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetro
                        command.Parameters.Add("p_id_usuario", OracleDbType.Int32).Value = idUsuario;

                        await command.ExecuteNonQueryAsync();

                        return Response<bool>.Done("Usuario eliminado exitosamente", true);
                    }
                }
            }
            catch (OracleException ex)
            {
                if (ex.Message.Contains("ORA-20007"))
                {
                    return Response<bool>.Fail("No se encontró el usuario a eliminar");
                }
                return Response<bool>.Fail($"Error en Oracle: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Response<bool>.Fail($"Error inesperado: {ex.Message}");
            }
        }
    }

    // DTO auxiliar para mapear el resultado del listar_usuarios_activos
    internal class UsuarioListaDTO
    {
        public int IdUsuario { get; set; }
        public string PrimerNombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string Correo { get; set; }
        public string TelefonoPrincipal { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string DireccionCompleta { get; set; }
        public string Ciudad { get; set; }
        public string Rol { get; set; }
    }
}