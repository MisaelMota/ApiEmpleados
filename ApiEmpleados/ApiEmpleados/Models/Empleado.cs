namespace ApiEmpleados.Models
{
    public class Empleado
    {
       
        public string Documento_Identidad { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Nacionalidad { get; set; }
        public string Telefono { get; set; }
        public string Sexo { get; set; }
        public DateTime Fecha_Nacimiento { get; set; }
        public decimal Salario { get; set; }
        public DateTime Fecha_Ingreso { get; set; }
        public int PuestoID { get; set; }
        public List<Hijo> Hijos { get; set; }

    }
}
