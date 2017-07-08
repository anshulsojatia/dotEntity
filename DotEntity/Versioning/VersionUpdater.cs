// #region Author Information
// // VersionRunner.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Linq;
using DotEntity.Constants;
using DotEntity.Reflection;
using DotEntity.Utils;

namespace DotEntity.Versioning
{
    public class VersionUpdater
    {
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

                    if (!transaction.Success)
                    {
                        throw new Exception("Couldn't setup version");
                    }
                }
            }

            //first get all the versions from database
            var appliedDatabaseVersions = EntitySet<DotEntityVersion>.Select().ToList();
            var availableVersions = TypeFinder.ClassesOfType<IDatabaseVersion>()
                .Select(x => (IDatabaseVersion)Instantiator.GetInstance(x));

            using (var transaction = EntitySet.BeginInternalTransaction())
            {
                foreach (var availableVersion in availableVersions)
                {
                    if (appliedDatabaseVersions.Any(x => x.VersionKey == availableVersion.VersionKey)) //already applied this one
                        continue;

                    //upgrade this
                    availableVersion.Upgrade(transaction);
                    var newVersion = new DotEntityVersion()
                    {
                        VersionKey = availableVersion.VersionKey
                    };
                    //insert the version
                    EntitySet<DotEntityVersion>.Insert(newVersion, transaction);
                }
                (transaction as DotEntityTransaction)?.CommitInternal();
            }
            
        }

        public void RunDowngrade(string versionKey = null)
        {
            DotEntityDb.MapTableNameForType<DotEntityVersion>(Configuration.VersionTableName);

            //do we have versioning table
            if (!DotEntityDb.Provider.IsDatabaseVersioned(Configuration.VersionTableName))
            {
                throw new Exception("The database is not versioned");
            }

            //first get all the versions from database
            var appliedDatabaseVersions = EntitySet<DotEntityVersion>.Select().ToList();

            if (versionKey != null)
            {
                if (appliedDatabaseVersions.All(x => x.VersionKey != versionKey))
                {
                    return; // do nothing. we don't have that version
                }
            }

            var availableVersions = TypeFinder.ClassesOfType<IDatabaseVersion>()
                .Select(x => (IDatabaseVersion) Instantiator.GetInstance(x)).Reverse(); //in reverse order


            using (var transaction = EntitySet.BeginInternalTransaction())
            {
                foreach (var availableVersion in availableVersions)
                {
                    if (versionKey == availableVersion.VersionKey)
                        break; //stop here. everything done
                    var vKey = availableVersion.VersionKey;
                    if (appliedDatabaseVersions.Any(x => versionKey != null && x.VersionKey == vKey)) //already applied this one
                        continue;

                    availableVersion.Downgrade(transaction);
                    //remove the version
                    EntitySet<DotEntityVersion>.Delete(x => x.VersionKey == vKey, transaction);
                }
                (transaction as DotEntityTransaction)?.CommitInternal();
            }

        }
    }
}