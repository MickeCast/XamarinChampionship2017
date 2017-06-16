#define OFFLINE_SYNC_ENABLED
using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Content;
using System.Collections.Generic;
using Android.Views;
using System.Threading.Tasks;
using System.IO;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Acr.UserDialogs;
using System.Linq;

namespace Recetario
{
    [Activity(Label = "Recetario", MainLauncher = true, Icon = "@drawable/unnamed")]
    public class MainActivity : ListActivity
    {
        Button bttAgregarReceta;
        Button bttRefrescar;
        List<recetas> recetasGuardadas;
        const string localDbFilename = "localstore.db";
        RecetasServiceHandler service = new RecetasServiceHandler();
        List<string> recetasDesc = new List<string>();
        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            UserDialogs.Init(this);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            bttAgregarReceta = (Button)FindViewById(Resource.Id.bttAgregarReceta);
            bttAgregarReceta.Click += NuevaRecetaOnClick;
            bttRefrescar = (Button)FindViewById(Resource.Id.bttRefrescarLista);
            bttRefrescar.Click += refrescarLista;
            await InitLocalStoreAsync();
            Plugin.Connectivity.CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
            OnRefreshItemsSelected();
            if (Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {
                recetasGuardadas = await service.BuscarRecetas();
                if (recetasGuardadas.Count > 0)
                {
                    foreach (recetas rec in recetasGuardadas)
                    {
                        if (!recetasDesc.Contains("Receta: " + rec.TituloReceta))
                        {
                            recetasDesc.Add("Receta: " + rec.TituloReceta);
                        }
                    }
                    ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, recetasDesc);
                }
                else
                {
                    Toast.MakeText(this, "Inicia agregando alguna receta para que se muestre en la lista", ToastLength.Long).Show();
                }
            }
            else
            {
                Toast.MakeText(this, "Necesitas una conexion a internet para consultar las recetas guardadas", ToastLength.Long).Show();
            }

        }

        protected override void OnListItemClick(ListView l, View v, int position, long id)
        {
            var t = recetasGuardadas[position];
            UserDialogs.Instance.Alert("Preparacion: " + t.ContenidoReceta, t.TituloReceta, "Cerrar");
            base.OnListItemClick(l, v, position, id);
        }


        public void NuevaRecetaOnClick(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(NuevaRecetaActivity));
            StartActivity(intent);
        }

        public async void refrescarLista(object sender, EventArgs e)
        {
            if (Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {
                recetasGuardadas = await service.BuscarRecetas();
                foreach (recetas rec in recetasGuardadas)
                {
                    if (!recetasDesc.Contains("Receta: " + rec.TituloReceta))
                    {
                        recetasDesc.Add("Receta: " + rec.TituloReceta);
                    }
                }
                ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, recetasDesc);
            }
            else
            {
                Toast.MakeText(this, "Necesitas una conexion a internet para consultar las recetas guardadas", ToastLength.Long).Show();
            }
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

        private void Current_ConnectivityChanged(object sender, Plugin.Connectivity.Abstractions.ConnectivityChangedEventArgs e)
        {
            if (Plugin.Connectivity.CrossConnectivity.Current.IsConnected)
            {
                Toast.MakeText(this, "Conectado a Internet", ToastLength.Long).Show();
                OnRefreshItemsSelected();
            }
            else
            {
                Toast.MakeText(this, "No hay una conexión disponible", ToastLength.Long).Show();
            }
        }

        private async Task SyncAsync()
        {
            try
            {
                await service.syncRecetasTable.PullAsync("allrecetas", service.syncRecetasTable.CreateQuery().Where(
                    item => item.TituloReceta != string.Empty));
            }
            catch (Exception e)
            {
                string error = e.Message;
            }
        }

        private async Task RefreshItemsFromTableAsync()
        {
            try
            {
                // Get the items that weren't marked as completed and add them in the adapter
                var resultado = await service.syncRecetasTable.ToListAsync();
                foreach (recetas rec in resultado)
                {
                    if (!recetasDesc.Contains("Receta: " + rec.TituloReceta))
                    {
                        recetasDesc.Add("Receta: " + rec.TituloReceta);
                    }
                }
                ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, recetasDesc);
            }
            catch (Exception e)
            {
                string error = e.Message;
            }
        }

        private async void OnRefreshItemsSelected()
        {
            await SyncAsync();
            await RefreshItemsFromTableAsync();
        }
    }
}

