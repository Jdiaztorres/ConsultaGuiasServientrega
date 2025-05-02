using MySql.Data.MySqlClient;
using ProyectoServientrega.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProyectoServientrega.Services
{
    public class MySqlService
    {
        private readonly string _connectionString;

        public MySqlService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public (List<GuiaInfo> guiasValidas, List<GuiaInfo> guiasInvalidas) ObtenerGuias()
        {
            var guiasValidas = new List<GuiaInfo>();
            var guiasInvalidas = new List<GuiaInfo>();

            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string sql = @"
                    SELECT guia, ods, envio, llegada
                    FROM movimiento
                    WHERE tipo = 0
                      AND (llegada IS NULL OR llegada = '')
                      AND envio >= CURDATE() - INTERVAL 90 DAY;
                ";

                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var guia = new GuiaInfo
                    {
                        NumeroGuia = reader["guia"].ToString().Trim(),
                        OrdenDeServicio = reader["ods"].ToString().Trim(),
                        FechaEnvio = reader["envio"].ToString().Trim(),
                        FechaLlegada = reader["llegada"].ToString().Trim()
                    };

                    if (Regex.IsMatch(guia.NumeroGuia ?? "", @"^\d{9,15}$"))
                        guiasValidas.Add(guia);
                    else
                        guiasInvalidas.Add(guia);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en base de datos: {ex.Message}");
            }

            return (guiasValidas, guiasInvalidas);
        }

        public void ActualizarFechaLlegada(string numeroGuia, string fechaEntrega)
        {
            try
            {
                using var conn = new MySqlConnection(_connectionString);
                conn.Open();

                string sql = @"
                    UPDATE movimiento
                    SET llegada = @fechaEntrega
                    WHERE guia = @numeroGuia
                      AND (llegada IS NULL OR llegada = '');
                ";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@fechaEntrega", fechaEntrega);
                cmd.Parameters.AddWithValue("@numeroGuia", numeroGuia);
                cmd.ExecuteNonQuery();
                Console.WriteLine($"✅ Fecha de entrega insertada en la guía {numeroGuia}: {fechaEntrega}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error actualizando guía {numeroGuia}: {ex.Message}");
            }
        }
    }
}
