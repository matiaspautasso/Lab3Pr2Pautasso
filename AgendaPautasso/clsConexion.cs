﻿using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.Collections;
using System.Data.Common;
using System.IO;


namespace AgendaPautasso
{
    internal class clsConexion
    {
        OleDbCommand comando;
        OleDbConnection conexion;
        OleDbDataAdapter adaptador;

        public static string cadena; //para poder llamarla desde el otro form
        public clsConexion()
        {
             cadena = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=.\BdAgenda\BdAgenda.accdb;";

            //recordar esta conexion ==> cadena = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=./Usuarios.mdb;";
        }
        public bool VerificarConexion()
        {
            bool result = false;

            conexion = new OleDbConnection(cadena);
            try
            {
                conexion.Open();
                result = true;
                MessageBox.Show("conectado");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally { conexion.Close(); }

            return result;
        }
        public void Agregar(string nombre, string apellido, string telefono, string correo, string categoria)
        {
            try
            {
                conexion = new OleDbConnection(cadena);
                comando = new OleDbCommand();
                comando.Connection = conexion;
                comando.CommandType = CommandType.Text;
                comando.CommandText = "INSERT INTO AGENDA (Nombre, Apellido, Telefono, Correo, Categoria) " +
                             "VALUES (@nombre, @apellido, @telefono, @correo, @categoria)";  //recordar estos son los valores que le paso
                                                                                             //a la base de datos por eso se escriben con @

                comando.Parameters.AddWithValue("@nombre", nombre);
                comando.Parameters.AddWithValue("@apellido", apellido);
                comando.Parameters.AddWithValue("@telefono", telefono);
                comando.Parameters.AddWithValue("@correo", correo);
                comando.Parameters.AddWithValue("@categoria", categoria);

                conexion.Open();
                comando.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conexion.Close();
            }

        }
        public void Eliminar(string nombre, string apellido)
        {
            try
            {
                conexion = new OleDbConnection(cadena);  // Inicializa la conexión
                comando = new OleDbCommand();  // Crea el comando
                comando.Connection = conexion;  // Asocia el comando con la conexión
                comando.CommandType = CommandType.Text;  // Tipo de comando: texto (consulta SQL)

                // Consulta SQL adaptada para eliminar según Nombre y Apellido
                comando.CommandText = "DELETE FROM AGENDA WHERE Nombre = @Nombre AND Apellido = @Apellido";

                // Asocia los parámetros para evitar inyecciones SQL y errores de tipo
                comando.Parameters.AddWithValue("@Nombre", nombre);
                comando.Parameters.AddWithValue("@Apellido", apellido);

                conexion.Open();  // Abre la conexión
                comando.ExecuteNonQuery();  // Ejecuta la consulta de eliminación
            }
            catch (Exception ex)
            {
                // Muestra un mensaje en caso de error
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // Asegura que la conexión se cierre siempre, haya error o no
                conexion.Close();
            }
        }
        public void MostrarGrilla(DataGridView grilla)
        {
            try
            {
                conexion = new OleDbConnection(cadena);
                comando = new OleDbCommand();
                comando.Connection = conexion;
                comando.CommandType = CommandType.Text;
                comando.CommandText = "SELECT * FROM AGENDA";
                DataTable TABLA = new DataTable();
                adaptador = new OleDbDataAdapter(comando);
                adaptador.Fill(TABLA);
                grilla.DataSource = TABLA;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conexion.Close();
            }
        }
        public void MostrarTreeView(TreeView treeViewCategorias)
        {
            try
            {
                // Limpiar el TreeView antes de agregar nuevos nodos
                treeViewCategorias.Nodes.Clear();

                // Usar la cadena de conexión ya configurada
                conexion = new OleDbConnection(cadena);
                comando = new OleDbCommand();
                comando.Connection = conexion;
                comando.CommandType = CommandType.Text;

                // Consulta SQL para obtener todos los contactos
                comando.CommandText = "SELECT Nombre, Apellido, Telefono, Correo, Categoria FROM AGENDA";

                // Abrir la conexión
                conexion.Open();

                // Crear un DataTable para almacenar los datos
                DataTable tabla = new DataTable();

                // Llenar el DataTable con los datos de la tabla AGENDA
                adaptador = new OleDbDataAdapter(comando);
                adaptador.Fill(tabla);

                // Crear un diccionario para agrupar los contactos por categoría
                Dictionary<string, List<string>> contactosPorCategoria = new Dictionary<string, List<string>>();

                // Iterar sobre las filas de la tabla para agrupar por categoría
                foreach (DataRow fila in tabla.Rows)
                {
                    // Obtener los valores de las columnas
                    string nombre = fila["Nombre"].ToString();
                    string apellido = fila["Apellido"].ToString();
                    string telefono = fila["Telefono"].ToString();
                    string correo = fila["Correo"].ToString();
                    string categoria = fila["Categoria"].ToString();

                    // Crear una cadena con los detalles del contacto
                    string contacto = $"{nombre} {apellido} - {telefono} - {correo}";

                    // Si la categoría no existe en el diccionario, añadirla
                    if (!contactosPorCategoria.ContainsKey(categoria))
                    {
                        contactosPorCategoria[categoria] = new List<string>();
                    }

                    // Agregar el contacto a la lista correspondiente a la categoría
                    contactosPorCategoria[categoria].Add(contacto);
                }

                // Cargar las categorías y contactos en el TreeView
                foreach (var categoria in contactosPorCategoria)
                {
                    // Crear un nodo para la categoría (nodo padre)
                    TreeNode nodoCategoria = new TreeNode(categoria.Key);

                    // Agregar los contactos como nodos hijos de la categoría
                    foreach (var contacto in categoria.Value)
                    {
                        TreeNode nodoContacto = new TreeNode(contacto);
                        nodoCategoria.Nodes.Add(nodoContacto);
                    }

                    // Agregar el nodo de la categoría al TreeView
                    treeViewCategorias.Nodes.Add(nodoCategoria);
                }
            }
            catch (Exception ex)
            {
                // Mostrar un mensaje en caso de error
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // Asegurarse de cerrar la conexión
                if (conexion != null)
                {
                    conexion.Close();
                }
            }
        }

        public void BuscarPorCategoria(string categoria, DataGridView dgv)
        {
            try
            {
                conexion = new OleDbConnection(cadena);
                comando = new OleDbCommand();
                comando.Connection = conexion;
                comando.CommandType = CommandType.Text;

                // Consulta SQL para buscar por categoría
                comando.CommandText = "SELECT * FROM AGENDA WHERE Categoria = @categoria";
                comando.Parameters.AddWithValue("@categoria", categoria);
                //recordar el uso de parametros con @ sirven para insertar ese argumento literalmente en la base 
                DataTable tabla = new DataTable();
                adaptador = new OleDbDataAdapter(comando);
                adaptador.Fill(tabla);

                dgv.DataSource = tabla; // Actualizar el DataGridView con los resultados
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conexion.Close();
            }
        }

        public void Exportar(string categoria, string tipoArchivo)
        {
            try
            {
                conexion = new OleDbConnection(cadena);
                comando = new OleDbCommand();
                comando.Connection = conexion;
                comando.CommandType = CommandType.Text;

                string consultaSql = "";
                if (categoria == "Todos")  //trae toda la tabla sin filtro 
                {
                    consultaSql = "SELECT * FROM AGENDA";
                }
                else
                {
                    consultaSql = "SELECT * FROM AGENDA WHERE Categoria = @categoria"; //trae en base a la categ selecc
                    comando.Parameters.AddWithValue("@categoria", categoria);
                }

                comando.CommandText = consultaSql;
                DataTable tabla = new DataTable();
                adaptador = new OleDbDataAdapter(comando);
                adaptador.Fill(tabla);

                if (tipoArchivo == "Csv")  
                {
                    StreamWriter writer = new StreamWriter("AGENDA.csv", false, Encoding.UTF8);
                    writer.WriteLine("IdContacto;Nombre;Apellido;Telefono;Correo;Categoria");
                    foreach (DataRow row in tabla.Rows)
                    {
                        writer.Write(row["IdContacto"].ToString());
                        writer.Write(";");
                        writer.Write(row["Nombre"].ToString());
                        writer.Write(";");
                        writer.Write(row["Apellido"].ToString());
                        writer.Write(";");
                        writer.WriteLine(row["Telefono"].ToString());
                        writer.Write(";");
                        writer.WriteLine(row["Correo"].ToString());
                        writer.Write(";");
                        writer.WriteLine(row["Categoria"].ToString());
                    }
                    writer.Close();
                }
                else if (tipoArchivo == "Excel")
                {
                    StreamWriter writer = new StreamWriter("AGENDA.xls", false, Encoding.UTF8);
                    writer.WriteLine("Categoria\tNombre\tApellido\tTelefono\tCorreo");
                    foreach (DataRow row in tabla.Rows)
                    {
                        writer.Write(row["Categoria"].ToString() + "\t"); // /t es para tabular en excel dejar en claro que es una columna 
                        writer.Write(row["Nombre"].ToString() + "\t");
                        writer.Write(row["Apellido"].ToString() + "\t");
                        writer.Write(row["Telefono"].ToString() + "\t");
                        writer.WriteLine(row["Correo"].ToString());
                    }
                     writer.Close ();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conexion.Close();
            }
        }



        public void BuscarPorTexto(string texto, DataGridView dgv)
        {
            try
            {
                conexion = new OleDbConnection(cadena);
                comando = new OleDbCommand();
                comando.Connection = conexion;
                comando.CommandType = CommandType.Text;

                // Consulta SQL para buscar por nombre, teléfono o correo
                comando.CommandText = "SELECT * FROM AGENDA WHERE Nombre LIKE @texto OR Telefono LIKE @texto OR Correo LIKE @texto";
                comando.Parameters.AddWithValue("@texto", "%" + texto + "%"); // Usar LIKE para buscar coincidencias de cadenas de texto 
                //recordar uso de % entre cadenas de texto para busqueda parcial ej : 
                //===> se busca %matias%, osea se busca matias dentro de la base 
                DataTable tabla = new DataTable();
                adaptador = new OleDbDataAdapter(comando);
                adaptador.Fill(tabla);

                dgv.DataSource = tabla; // Actualizar el DataGridView con los resultados
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conexion.Close();
            }
        }
        public void BuscarPorCategoriaYTexto(string categoria, string texto, DataGridView dgv)
        {
            try
            {
                conexion = new OleDbConnection(cadena);
                comando = new OleDbCommand();
                comando.Connection = conexion;
                comando.CommandType = CommandType.Text;

                // Consulta SQL para buscar por categoría y también nombre, teléfono o correo
                comando.CommandText = "SELECT * FROM AGENDA WHERE Categoria = @categoria AND (Nombre LIKE @texto OR Telefono LIKE @texto OR Correo LIKE @texto)";
                comando.Parameters.AddWithValue("@categoria", categoria);
                comando.Parameters.AddWithValue("@texto", "%" + texto + "%"); // Usar LIKE para búsquedas parciales
                //recordar uso % EJ : buscar matias ===> %matias% 
                DataTable tabla = new DataTable();
                adaptador = new OleDbDataAdapter(comando);
                adaptador.Fill(tabla);

                dgv.DataSource = tabla; // Actualizar el DataGridView con los resultados
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conexion.Close();
            }

        }
    }
}
