#nullable disable
using System;
using System.Collections.Generic;

namespace SantiagoConectaIA.DAL.Models;

public partial class Grupo
{
    public int IdGrupo { get; set; }

    public string Nombre { get; set; }

    public string Alias { get; set; }

    public virtual ICollection<Catalogo> Catalogos { get; set; } = new List<Catalogo>();
}
