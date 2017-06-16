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
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace Recetario
{
    public class RecetasServiceHandler
    {
        MobileServiceClient client;

        IMobileServiceTable<recetas> recetaTable;

        //Mobile Service sync table used to access data
        public IMobileServiceSyncTable<recetas> syncRecetasTable;


        public RecetasServiceHandler()
        {
            try
            {
                this.client = new MobileServiceClient(@"https://recetariochampionship.azurewebsites.net");
                this.recetaTable = client.GetTable<recetas>();
                this.syncRecetasTable = client.GetSyncTable<recetas>();
            }
            catch (Exception ex)
            {
                //TOTO manejar Error                              
            }
        }


        public MobileServiceClient CurrentClient
        {
            get
            {
                return client;
            }
        }

        public async Task<List<recetas>> BuscarRecetas()
        {
            List<recetas> recs = new List<recetas>();
            try
            {
                recs = await recetaTable.ToListAsync();
            }
            catch (Exception ex)
            {
            }
            return recs;
        }


        public async Task SaveTaskAsync(recetas rec)
        {
            if (rec.TituloReceta != null)
            {
                try
                {
                    
                        await recetaTable.InsertAsync(rec);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}