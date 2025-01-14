﻿using System;
using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.Transportation
{

    public class SiteLocationTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SiteLocationTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Location loc = new Location("loc1", null, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                loc.SaveToDB();
                SiteLocation sl = new SiteLocation(null, loc, -1, db.ConnectionString, "name", Guid.NewGuid().ToStrGuid());
                ObservableCollection<SiteLocation> slocs = new ObservableCollection<SiteLocation>();
                ObservableCollection<Location> locs = new ObservableCollection<Location>();
                sl.SaveToDB();
                locs.Add(loc);
                SiteLocation.LoadFromDatabase(slocs, db.ConnectionString, locs, false);
                db.Cleanup();
                (slocs.Count).Should().Be(1);
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SiteWithLocationTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(Site.TableName);
                db.ClearTable(SiteLocation.TableName);
                Location loc = new Location("loc1", null, db.ConnectionString, Guid.NewGuid().ToStrGuid());
                loc.SaveToDB();
                Site site = new Site("site1", null, db.ConnectionString, "desc", true, Guid.NewGuid().ToStrGuid());
                site.SaveToDB();
                site.AddLocation(loc);
                //loading
                ObservableCollection<Site> slocs = new ObservableCollection<Site>();
                ObservableCollection<Location> locs = new ObservableCollection<Location>
            {
                loc
            };
                Site.LoadFromDatabase(slocs, db.ConnectionString,
                    false, locs);
                db.Cleanup();
                (slocs.Count).Should().Be(1);
            }
        }

        public SiteLocationTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}