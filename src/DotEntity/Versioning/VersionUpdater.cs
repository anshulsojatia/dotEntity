/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (VersionUpdater.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
 * 
 * dotEntity is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 
 * dotEntity is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU Affero General Public License for more details.
 
 * You should have received a copy of the GNU Affero General Public License
 * along with dotEntity.If not, see<http://www.gnu.org/licenses/>.

 * You can release yourself from the requirements of the AGPL license by purchasing
 * a commercial license (dotEntity Pro). Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/licensing
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotEntity.Constants;
using DotEntity.Reflection;

namespace DotEntity.Versioning
{
    public class VersionUpdater
    {
        private readonly string _callingContextName;
        private readonly IDatabaseVersion[] _databaseVersions;

        public VersionUpdater(string callingContextName, IDatabaseVersion[] databaseVersions)
        {
            _callingContextName = callingContextName;
            _databaseVersions = databaseVersions;
        }

        public VersionUpdater(string callingContextName) : this(callingContextName, null)
        {
            
        }

        public VersionUpdater()
        {

        }

        public void RunUpgrade()
        {
            DotEntityDb.MapTableNameForType<DotEntityVersion>(Configuration.VersionTableName);

            //do we have versioning table
            if (!DotEntityDb.Provider.IsDatabaseVersioned(Configuration.VersionTableName))
            {
                //we'll have to setup the version table first
                using (var transaction = EntitySet.BeginTransaction())
                {
                    DotEntity.Database.CreateTable<DotEntityVersion>(transaction);
                    transaction.Commit();
                    DotEntity.Database.ResetProcessedTables();

                    if (!transaction.Success)
                    {
                        throw new Exception("Couldn't setup version");
                    }
                }
            }

            //first get all the versions from database
            var appliedDatabaseVersions = EntitySet<DotEntityVersion>.Where(x => x.ContextName == _callingContextName).Select().ToList();

            using (var transaction = EntitySet.BeginInternalTransaction())
            {
                foreach (var availableVersion in _databaseVersions)
                {
                    if (appliedDatabaseVersions.Any(x => x.VersionKey == availableVersion.VersionKey)) //already applied this one
                        continue;

                    //upgrade this
                    availableVersion.Upgrade(transaction);
                    var newVersion = new DotEntityVersion()
                    {
                        VersionKey = availableVersion.VersionKey,
                        ContextName = _callingContextName
                    };
                    //insert the version
                    EntitySet<DotEntityVersion>.Insert(newVersion, transaction);
                }
                (transaction as DotEntityTransaction)?.CommitInternal();
                DotEntity.Database.ResetProcessedTables();
            }
            
        }

        public void RunDowngrade(string versionKey = null)
        {
            DotEntityDb.MapTableNameForType<DotEntityVersion>(Configuration.VersionTableName);
            Throw.IfDbNotVersioned(!DotEntityDb.Provider.IsDatabaseVersioned(Configuration.VersionTableName));
           
            //first get all the versions from database
            var appliedDatabaseVersions = EntitySet<DotEntityVersion>.Where(x => x.ContextName == _callingContextName).Select().ToList();

            if (versionKey != null)
            {
                if (appliedDatabaseVersions.All(x => x.VersionKey != versionKey))
                {
                    return; // do nothing. we don't have that version
                }
            }

            using (var transaction = EntitySet.BeginInternalTransaction())
            {
                foreach (var availableVersion in _databaseVersions.Reverse())
                {
                    if (versionKey == availableVersion.VersionKey)
                        break; //stop here. everything done
                    var vKey = availableVersion.VersionKey;
                    if (appliedDatabaseVersions.Any(x => versionKey != null && x.VersionKey == vKey)) //already applied this one
                        continue;

                    availableVersion.Downgrade(transaction);
                    //remove the version
                    EntitySet<DotEntityVersion>.Delete(x => x.VersionKey == vKey && x.ContextName == _callingContextName, transaction);
                }
                (transaction as DotEntityTransaction)?.CommitInternal();
            }

        }

        public IList<string> GetAppliedVersions()
        {
            //first get all the versions from database
            var appliedDatabaseVersions =
                EntitySet<DotEntityVersion>.Where(x => x.ContextName == _callingContextName).Select();
            return appliedDatabaseVersions.Select(x => x.VersionKey).ToList();
        }

        public static IDictionary<string, List<string>> GetAllAppliedVersions()
        {
            return EntitySet<DotEntityVersion>.Where(x => true).Select().GroupBy(x => x.ContextName)
                .ToDictionary(x => x.Key, x => x.Select(y => y.VersionKey).ToList());
        }
    }
}