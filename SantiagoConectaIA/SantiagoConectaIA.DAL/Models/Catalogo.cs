#nullable disable
using System;
using System.Collections.Generic;

namespace SantiagoConectaIA.DAL.Models;

public partial class Catalogo
{
    public int IdCatalogo { get; set; }

    public int IdGrupo { get; set; }

    public string Descripcion { get; set; }

    public string Valor { get; set; }

    public virtual Grupo Grupo { get; set; }
}
