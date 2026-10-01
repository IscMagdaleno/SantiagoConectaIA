namespace SantiagoConectaIA.Share.Utilities
{
    public sealed record PromptImagen(string Comando, string Titulo, string Prompt);

    public static class PromptsImagen
    {
        public static IReadOnlyList<PromptImagen> Lista { get; } = new List<PromptImagen>
        {
            new("/editorial", "Editorial",
                "Transforma y edita esta imagen para que adopte el estilo visual sofisticado y dramático típico de una sesión fotográfica para una revista de alta costura o diseño. Ajusta la iluminación de estudio de alta gama, realza los contrastes de manera sutil, cuida la composición para que transmita una narrativa visual compleja y asegúrate de que los colores y texturas tengan un acabado profesional, impecable y de publicación impresa."),
            new("/luxury", "Lujo",
                "Modifica esta imagen para elevar su estética general hacia un concepto visual de lujo y exclusividad absoluta. Enriquece los tonos cromáticos con acentos dorados o profundos, resalta las texturas de los materiales para que luzcan costosos y de primera calidad, e implementa una iluminación envolvente y suave que evoque elegancia, opulencia y un estándar estético sumamente refinado y distinguido."),
            new("/ugc", "Contenido de usuario",
                "Ajusta la apariencia visual de esta imagen para que parezca contenido generado por usuarios (UGC). Haz que luzca auténtica, espontánea y cercana, simulando haber sido capturada de forma casual con un teléfono móvil cotidiano, con una iluminación natural y realista que evite cualquier sensación de producción de estudio o de retoque artificial excesivo."),
            new("/selfie", "Selfie",
                "Reimagina la composición de esta imagen para transformarla en una autofoto o autorretrato estilo selfie. Modifica la perspectiva visual para simular la distancia típica de un brazo extendido o el uso de una cámara frontal, integrando un enfoque más cercano al sujeto con una profundidad de campo ligeramente reducida en el fondo circundante."),
            new("/headshot", "Retrato profesional",
                "Adapta esta imagen para convertirla en un retrato de tipo corporativo o profesional (headshot). Enfoca el encuadre firmemente desde los hombros o el pecho hacia arriba, asegurando una iluminación frontal limpia y favorecedora, un fondo neutro o sutilmente difuminado y una nitidez impecable que proyecte confianza, seriedad y profesionalismo institucional."),
            new("/rawphoto", "Foto natural",
                "Procesa esta imagen eliminando cualquier efecto de estilización exagerada, filtros pesados o retoques digitales evidentes, devolviéndole un aspecto puramente natural, realista y orgánico. Conserva fielmente la textura original de la piel, los elementos y el entorno tal como habrían sido capturados directamente por el sensor de la cámara sin alteraciones."),
            new("/macro", "Macro",
                "Realiza un acercamiento extremo de tipo fotográfico macro en esta imagen, enfocando con absoluta precisión los detalles más diminutos, las texturas intrincadas y los microelementos que normalmente pasarían desapercibidos a simple vista, generando un fondo suavemente difuminado que realce la profundidad y la nitidez del sujeto central."),
            new("/nightphoto", "Foto nocturna",
                "Modifica la iluminación y la atmósfera ambiental de esta imagen para transformarla en una auténtica fotografía nocturna. Incorpora sombras profundas, luces urbanas o ambientales puntuales con reflejos realistas y ajusta la paleta de colores para reflejar las condiciones de poca luz características de la noche con total naturalidad."),
            new("/flash", "Flash directo",
                "Modifica la escena de esta imagen para simular el impacto visual característico de una fotografía tomada con un flash directo de cámara en un entorno oscuro o cerrado. Genera sombras duras y definidas proyectadas justo detrás de los sujetos u objetos, con una iluminación frontal intensa que resalte los brillos directos."),
            new("/goldenhour", "Hora dorada",
                "Aplica una iluminación ambiental basada en la hora dorada (golden hour), inundando la escena con tonos cálidos, dorados, anaranjados y suaves. Genera destellos de luz solar sutiles y sombras alargadas que transmitan una atmósfera nostálgica, apacible, romántica y estéticamente muy atractiva."),
            new("/moody", "Dramático",
                "Transforma la atmósfera emocional y visual de esta imagen hacia un estilo moody, caracterizado por una paleta de colores desaturada o fría, sombras profundas y densas, y un contraste marcado que transmita una fuerte carga dramática, misterio, introspección y una narrativa visual sumamente envolvente y seria."),
            new("/minimal", "Minimalista",
                "Simplifica de manera radical la composición de esta imagen bajo el concepto del minimalismo estético. Elimina elementos distractores o recargados, deja espacios negativos amplios, utiliza líneas limpias y colores neutros o sólidos para que la atención se concentre exclusivamente en la esencia pura de la forma principal."),
            new("/fashion", "Moda",
                "Modifica la imagen para otorgarle un estilo estético de alta costura y fotografía de moda contemporánea. Perfecciona el modelado de la luz para esculpir las formas, realza la textura de la ropa y los elementos circundantes con un dramatismo controlado, y dota a la toma de una elegancia vanguardista digna de pasarela."),
            new("/foodphoto", "Gastronomía",
                "Convierte la presentación visual de esta imagen en una fotografía gastronómica profesional de alta gama (food photo). Ajusta el brillo, el balance de color y los reflejos para que los alimentos y bebidas luzcan sumamente frescos, apetitosos, con texturas perfectamente definidas y una iluminación culinaria especializada."),
            new("/productpremium", "Producto premium",
                "Optimiza esta imagen para que funcione como una fotografía de producto comercial de categoría premium. Diseña un esquema de iluminación de estudio controlado con reflejos limpios sobre las superficies, elimina imperfecciones y resalta los acabados del objeto para transmitir máxima calidad, valor y deseo de adquisición."),
            new("/flatlay", "Vista cenital",
                "Reorganiza la disposición visual de los elementos en esta imagen para presentar una toma cenital perfecta (flat lay), donde todos los objetos estén ordenados y fotografiados rigurosamente desde arriba, en un ángulo plano de 90 grados sobre una superficie limpia que permita apreciar la simetría y el diseño."),
            new("/workspace", "Espacio de trabajo",
                "Integra los elementos principales de esta imagen dentro de un contexto de mesa de trabajo o estudio (workspace) moderno y organizado. Rodéalos de accesorios estéticos acordes al entorno profesional, como cuadernos, dispositivos tecnológicos o elementos de diseño, con una iluminación ambiental armónica y realista."),
            new("/sketch", "Boceto a lápiz",
                "Transforma por completo esta imagen en un dibujo artístico a lápiz o boceto monocromático (sketch). Destaca las líneas de contorno principales, añade trazos de sombreado cruzado simulados a mano alzada y haz que parezca un trabajo preliminar realizado sobre una hoja de papel texturizada."),
            new("/watercolor", "Acuarela",
                "Pinta y convierte esta imagen adoptando un estilo artístico de acuarela fluida. Reemplaza los bordes rígidos por manchas de color translúcidas, efectos de mezcla de pigmentos sobre papel húmedo, sutiles salpicaduras y texturas acuosas que otorguen un aspecto pictórico, artístico y delicado."),
            new("/comic", "Cómic",
                "Vuelve esta imagen una ilustración de estilo cómic tradicional. Aplica líneas de tinta negras bien definidas, tramas de puntos estilo Ben-Day para los tonos intermedios y colores planos o saturados que evoquen la estética clásica de las viñetas impresas de historietas gráficas."),
            new("/anime", "Anime",
                "Adapta esta imagen para que adquiera un estilo visual de animación japonesa (anime). Modifica los rasgos, los contornos y las dinámicas de color para reflejar la estética característica de la cultura otaku, incluyendo destellos lumínicos en los ojos u objetos y un sombreado digital típico de cel-shading."),
            new("/sticker", "Sticker",
                "Convierte el sujeto u objeto principal de esta imagen en el diseño de una pegatina o calcomanía (sticker) troquelada. Añade un borde o contorno exterior blanco grueso y uniforme alrededor de toda la figura, con una sutil sombra paralela que genere un efecto tridimensional despegado del fondo."),
            new("/infographic", "Infografía",
                "Organiza y reestructura la información o los elementos visuales de esta imagen en un formato de infografía clara y estructurada. Distribuye los datos mediante secciones lógicas, iconos descriptivos, esquemas visuales limpios y una jerarquía tipográfica moderna que facilite la lectura rápida y comprensible."),
            new("/beforeafter", "Antes y después",
                "Diseña una composición visual comparativa de antes y después (before/after) dividida claramente en dos secciones simétricas o separadas por una línea central vertical, permitiendo contrastar de forma directa y evidente el estado inicial de la imagen frente a su versión transformada y final.")
        };
    }
}
