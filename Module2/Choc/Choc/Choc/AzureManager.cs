using Choc;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Choc
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<WalTable> walTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("https://walapp.azurewebsites.net/");
            this.walTable = this.client.GetTable<WalTable>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task<List<WalTable>> GetChocInformation()
        {
            return await this.walTable.ToListAsync();
        }

        public async Task PostChocInformation(WalTable walModel)
        {
            await this.walTable.InsertAsync(walModel);
        }

    }
}
