using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;

namespace Recetario
{
    public class recetas
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "TituloReceta")]
        public string TituloReceta { get; set; }
        [JsonProperty(PropertyName = "ContenidoReceta")]
        public string ContenidoReceta { get; set; }
        [Version]
        public string Version { get; set; }
    }
}