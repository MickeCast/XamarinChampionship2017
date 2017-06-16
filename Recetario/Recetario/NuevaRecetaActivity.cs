#define OFFLINE_SYNC_ENABLED
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
using System.Threading.Tasks;
using System.IO;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace Recetario
{
    [Activity(Label = "Agregar Receta")]
    public class NuevaRecetaActivity : Activity
    {
        Button bttRegistrarReceta;
        EditText txtTitulo;
        RecetasServiceHandler service = new RecetasServiceHandler();
        EditText txtContenido;
        const string localDbFilename = "localstore.db";
        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.nuevaRecetaLayout);
            bttRegistrarReceta = (Button)FindViewById(Resource.Id.bttRegistrarReceta);
            txtTitulo = (EditText)FindViewById(Resource.Id.txtTituloReceta);
            txtContenido = (EditText)FindViewById(Resource.Id.txtContenidoReceta);
            bttRegistrarReceta.Click += onRegistrarClick;
            await InitLocalStoreAsync();
        }

        private async Task InitLocalStoreAsync()
        {
            string path = Path.Combine(System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.Personal), localDbFilename);
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            var store = new MobileServiceSQLiteStore(path);
            store.DefineTable<recetas>();
            await service.CurrentClient.SyncContext.InitializeAsync(store);
        }

        public async void onRegistrarClick(object sender, EventArgs e)
        {
            if (Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {

                bool valido = true;
                if (string.IsNullOrEmpty(txtTitulo.Text))
                {
                    Toast.MakeText(this, "Por favor agrega el titulo de la receta", ToastLength.Short).Show();
                    valido = false;
                }
                else if (string.IsNullOrEmpty(txtContenido.Text))
                {
                    Toast.MakeText(this, "Por favor agrega el contenido de la receta", ToastLength.Short).Show();
                    valido = false;
                }
                if (valido)
                {
                    recetas receta = new recetas();
                    receta.Id = Guid.NewGuid().ToString();
                    receta.TituloReceta = txtTitulo.Text;
                    receta.ContenidoReceta = txtContenido.Text;
                    RecetasServiceHandler service = new RecetasServiceHandler();
                    await service.SaveTaskAsync(receta);

                    Toast.MakeText(this, "Receta Guardada", ToastLength.Short).Show();
                    this.Finish();
                }
            }
            else
            {
                Toast.MakeText(this, "La receta solo se guardará local, necesitas internet para guardarla en la nube", ToastLength.Long).Show();
            }
        }
    }

}
