﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using System.IO;
using CompassMobile.Models;

namespace CompassMobileUpdate.Models
{
    public class LocalSql
    {
        // I updated the methods in this local storage class to be asyncronous.
        // The CreateConnection method needs to be called from each method in the LocalSql class
        // thus avoiding a db locking strategy and having to call the CreateConnection method
        // in the constructor. KLH
        private SQLiteAsyncConnection _database;

        public LocalSql()
        {
        }

        private async Task CreateConnection()
        {
            if (_database != null) { return; }

            // we need to put in /Library/ on iOS5.1+ to meet Apple's iCloud terms
            // (they don't want non-user-generated data in Documents)
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            var databasePath = Path.Combine(documentsPath, "COMPASSMobile.db");

            _database = new SQLiteAsyncConnection(databasePath);

            await _database.CreateTableAsync<AppUser>();
            await _database.CreateTableAsync<LastUserID>();
            await _database.CreateTableAsync<LocalMeter>();
            await _database.CreateTableAsync<LocalVoltageRule>();
        }

        public async Task<bool> AddUser(AppUser appUser)
        {
            await CreateConnection();
            await _database.DeleteAllAsync<AppUser>();
            await _database.InsertAsync(appUser);          
            return true;
        }

        public async Task<bool> DeleteUsers()
        {
            await CreateConnection();
            await _database.DeleteAllAsync<AppUser>();
            return true;
        }

        public AppUser GetAppUser()
        {
            CreateConnection();

            var appUsers = _database.Table<AppUser>().ToListAsync().Result;

            AppUser appUser = new AppUser();

            if (appUsers.Count > 0)
            {
                appUser = appUsers[0];
                return appUser;
            }
            else
            {
                return null;
            }        
        }

        public async Task<DateTime> GetLastVoltageSyncTime()
        {
            await CreateConnection();

            DateTime result = DateTime.MinValue;

            if (await _database.Table<LocalVoltageRule>().CountAsync() > 0)
            {
                var localVoltageRule = await _database.Table<LocalVoltageRule>().OrderByDescending(x => x.CreatedTime).FirstOrDefaultAsync();
                result = localVoltageRule.CreatedTime;
            }
            return result;
        }

        private async Task<List<LocalMeter>> GetNonFavoriteMeters()
        {
            List<LocalMeter> localMeters = await _database.Table<LocalMeter>()
                .Where(m => m.IsFavorite == false)
                .OrderByDescending(m => m.LastAccessedTime).ToListAsync();
            return localMeters;
        }

        public async Task<List<LocalVoltageRule>> GetVoltageRules()
        {
            await CreateConnection();
            var localVoltageRules = await _database.Table<LocalVoltageRule>().ToListAsync();
            return localVoltageRules;
        }

        public async Task<bool> AddMeter(Meter meter)
        {
            await CreateConnection();
            if (meter != null)
            {
                LocalMeter meterLocal = LocalMeter.GetLocalMeterFromMeter(meter);
                meterLocal.CreatedTime = DateTime.Now;
                meterLocal.LastAccessedTime = DateTime.Now;
                meterLocal.LastUpdatedTime = DateTime.Now;
                try
                {
                    List<LocalMeter> meters = await GetNonFavoriteMeters();
                    int count = meters.Count;
                    if (count > 20)
                    {
                        List<DateTime> dateTimes = new List<DateTime>();
                        meters.Sort((x, y) => y.LastAccessedTime.CompareTo(x.LastAccessedTime));
                        for (int i = 20; i < meters.Count; i++)
                        {
                            await _database.DeleteAsync<LocalMeter>(meters[i].DeviceUtilityID);
                        }
                    }
                    await _database.InsertAsync(meterLocal);
                    return true;
                }
                catch (Exception e)
                {
                    //TODO: Add application logging
                    //AppVariables.AppService.LogApplicationError("LocalSql.cs", e);
                    return false;
                }
            }
            else
            {
                return false;
            }
        } // end AddMeter

        public async Task<bool> AddOrUpdateMeterLastAccessedTime(Meter meter)
        {
            await CreateConnection();
            if (_database.Table<LocalMeter>().Where(m => m.DeviceUtilityID == meter.DeviceUtilityID).CountAsync().Result == 0)
            {
                try
                {
                    await AddMeter(meter);
                    return true;
                }
                catch (Exception e)
                {
                    //TODO: Add application error logging
                    //AppVariables.AppService.LogApplicationError("LocalSql.cs", e);
                    return false;
                }
            }
            else
            {
                return await UpdateMeterLastAccessedTime(meter.DeviceUtilityID);
            }
        }

        public async Task<LocalMeter> GetLocalMeter(string deviceUtilityID)
        {
            await CreateConnection();
            var localMeter = await _database.Table<LocalMeter>().Where(meter => meter.DeviceUtilityID == deviceUtilityID).FirstOrDefaultAsync();
            return localMeter;
        }

        public async Task<List<LocalMeter>> GetLocalMetersSorted()
        {
            await CreateConnection();
            return await _database.Table<LocalMeter>().OrderByDescending(m => m.IsFavorite).OrderByDescending(m => m.LastAccessedTime).ToListAsync();
        }

        public async Task<DateTime> GetMeterLastUpdatedTime(string deviceUtilityID)
        {
            await CreateConnection();
            DateTime result = DateTime.MinValue;
            LocalMeter meter = await GetLocalMeter(deviceUtilityID);

            if (meter != null)
            {
                if (!string.IsNullOrWhiteSpace(meter.DeviceUtilityID))
                {
                    result = meter.LastUpdatedTime;
                }
            }
            return result;
        }

        public async Task<bool> UpdateMeterLastUpdatedTimeAsync(string deviceUtilityID)
        {
            await CreateConnection();
            try
            {
                var localMeter = await _database.Table<LocalMeter>().Where(meter => meter.DeviceUtilityID == deviceUtilityID).FirstOrDefaultAsync();
                localMeter.LastUpdatedTime= DateTime.Now;
                await _database.UpdateAsync(localMeter);
                return true;
            }
            catch (Exception e)
            {
                //TODO: Add application error logging
                //AppVariables.AppService.LogApplicationError("LocalSql.cs", e);
                return false;
            }
        }

        public async Task<bool> UpdateLocalMeterCustomerInformation(Meter meter)
        {
            await CreateConnection();
            try
            {
                LocalMeter existingMeter = await _database.GetAsync<LocalMeter>(meter.DeviceUtilityID);
                LocalMeter localMeter = LocalMeter.GetLocalMeterFromMeter(meter);

                if (existingMeter != null)
                {
                    localMeter.IsFavorite = existingMeter.IsFavorite;
                    localMeter.CreatedTime = existingMeter.CreatedTime;
                    localMeter.LastAccessedTime = existingMeter.LastAccessedTime;
                    localMeter.LastUpdatedTime = DateTime.Now;

                    await _database.UpdateAsync(localMeter);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                //TODO: Add app logging
                //AppVariables.AppService.LogApplicationError("LocalSql.cs", e);
                return false;
            }
        }

        public async Task<bool> ResetVoltageRules(List<LocalVoltageRule> voltageRules, bool usedHardCodedValues)
        {
            await CreateConnection();
            try
            {
                await _database.DeleteAllAsync<LocalVoltageRule>();
                DateTime createdTime = DateTime.Now;

                //We have some hardcoded values we can use if the service fails.
                //in that event set the CreatedDate to minimum so on next app service it overwrites them
                if (usedHardCodedValues)
                {
                    createdTime = DateTime.MinValue;
                }

                voltageRules.ForEach(x => x.CreatedTime = createdTime);
                await _database.InsertAllAsync(voltageRules, true);
                return true;
            }
            catch (Exception e)
            {
                //AppVariables.AppService.LogApplicationError("LocalSQl.cs", e);
                return false;
            }
        }

        public async void SetLastUserID(string userID)
        {
            await CreateConnection();
            //await _database.DeleteAllAsync<LastUserID>();
            var lastUserId = new LastUserID();
            lastUserId.UserID = userID;
            await _database.InsertAsync(lastUserId);
        }

        public async Task<bool> UpdateAllMeterLastUpdatedTimeToMin()
        {
            await CreateConnection();
            try
            {
                string sql = "update [LocalMeter] set LastUpdatedTime = ?";
                await _database.ExecuteAsync(sql, DateTime.MinValue);
                return true;
            }
            catch (Exception e)
            {
                //TODO: Add app logging
                //AppVariables.AppService.LogApplicationError("LocalSql.cs", e);
                return false;
            }
        }

        public async Task<bool> UpdateMeterIsFavorite(string deviceUtilityID, bool isFavorite)
        {
            await CreateConnection();
            try
            {
                var localMeter = await _database.Table<LocalMeter>().Where(meter => meter.DeviceUtilityID == deviceUtilityID).FirstOrDefaultAsync();
                if (localMeter != null)
                {
                    localMeter.IsFavorite = isFavorite;
                    await _database.UpdateAsync(localMeter);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                //TODO: Add application error logging
                //AppVariables.AppService.LogApplicationError("LocalSql.cs", e);
                return false;
            }
        }

        private async Task<bool> UpdateMeterLastAccessedTime(string deviceUtilityID)
        {
            await CreateConnection();
            try
            {
                var localMeter = await _database.Table<LocalMeter>().Where(meter => meter.DeviceUtilityID == deviceUtilityID).FirstOrDefaultAsync();
                localMeter.LastAccessedTime = DateTime.Now;
                await _database.UpdateAsync(localMeter);
                return true;
            }
            catch (Exception e)
            {
                //TODO: Add application error logging
                //AppVariables.AppService.LogApplicationError("LocalSql.cs", e);
                return false;
            }
        }
    }
}

