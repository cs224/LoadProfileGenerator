﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using Automation;
using Automation.ResultFiles;
using CalculationController.CalcFactories;
using CalculationController.Queue;
using Common;
using Common.Tests;
using Database;
using Database.Tables;
using Database.Tests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;


namespace CalculationController.Tests
{
    public class PostProcessingTests : UnitTestBaseClass
    {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [JetBrains.Annotations.NotNull]
        private static string GetCurrentMethod() {
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            return sf?.GetMethod()?.Name ??"No stack frame";
        }

        private static void RunTest([JetBrains.Annotations.NotNull] Action<GeneralConfig> setOption, [JetBrains.Annotations.NotNull] string name)
        {
            CleanTestBase.RunAutomatically(false);
            using (var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass() + name))
            {
                Logger.Threshold = Severity.Error;
                var path = wd1.WorkingDirectory;

                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                Directory.CreateDirectory(path);
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString);
                    Config.IsInUnitTesting = true;
                    Config.ExtraUnitTestChecking = false;
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.NoFiles);
                    sim.MyGeneralConfig.WriteExcelColumn = "False";
                    //if (setOption == null) { throw new LPGException("Action was null."); }
                    setOption(sim.MyGeneralConfig);

                    Logger.Info("Temperature:" + sim.MyGeneralConfig.SelectedTemperatureProfile);
                    Logger.Info("Geographic:" + sim.MyGeneralConfig.GeographicLocation);
                    sim.Should().NotBeNull();
                    var cmf = new CalcManagerFactory();
                    CalculationProfiler calculationProfiler = new CalculationProfiler();
                    CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                        sim.TemperatureProfiles[0], sim.ModularHouseholds[0], EnergyIntensityType.Random,
                        false, null, LoadTypePriority.All, null, null, null,
                        sim.MyGeneralConfig.AllEnabledOptions(), new DateTime(2018, 1, 1), new DateTime(2018, 1, 2), new TimeSpan(0, 1, 0),
                        ";", 5, new TimeSpan(0, 10, 0), false, false, 3, 3, calculationProfiler,
                        wd1.WorkingDirectory, false, false, ".", false);
                    var cm = cmf.GetCalcManager(sim,  csps, false);

                    static bool ReportCancelFunc1()
                    {
                        Logger.Info("canceled");
                        return true;
                    }
                    cm.Run(ReportCancelFunc1);
                    db.Cleanup();
                }
                wd1.CleanUp();
            }
            CleanTestBase.RunAutomatically(true);
        }

        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void PostProcessingTestCompareAllResultFiles()
        {
            double myParse(string s, NumberFormatInfo nfi)
            {
                if (s.Length > 3 && !s.Contains(nfi.NumberDecimalSeparator)) {
                    throw new LPGException("no decimal: " + s);
                }
                var d = double.Parse(s, nfi);
                return d;
            }

            //base.SkipEndCleaning = true;
            CleanTestBase.RunAutomatically(false);
            Config.ReallyMakeAllFilesIncludingBothSums = true;

            var start = DateTime.Now;
            using (var wd1 = new WorkingDir(Utili.GetCurrentMethodAndClass())) {
                Logger.Threshold = Severity.Error;
                var path = wd1.WorkingDirectory;

                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                Directory.CreateDirectory(path);
                using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
                {
                    var sim = new Simulator(db.ConnectionString);
                    Config.IsInUnitTesting = true;
                    Config.ExtraUnitTestChecking = true;
                    sim.MyGeneralConfig.ApplyOptionDefault(OutputFileDefault.NoFiles);
                    sim.MyGeneralConfig.WriteExcelColumn = "False";
                    const string decimalSep = ":";
                    sim.MyGeneralConfig.CSVCharacter = ";";
                    //sim.MyGeneralConfig.Enable(CalcOption.DeviceProfilesIndividualHouseholds);
                    //sim.MyGeneralConfig.Enable(CalcOption.DeviceProfilesHouse);
                    //sim.MyGeneralConfig.Enable(CalcOption.HouseSumProfilesFromDetailedDats);
                    //sim.MyGeneralConfig.Enable(CalcOption.DeviceProfileExternalEntireHouse);
                    //sim.MyGeneralConfig.Enable(CalcOption.SumProfileExternalEntireHouse);
                    //sim.MyGeneralConfig.Enable(CalcOption.SumProfileExternalIndividualHouseholds);
                    //sim.MyGeneralConfig.Enable(CalcOption.PolysunImportFiles);
                    //sim.MyGeneralConfig.Enable(CalcOption.HouseSumProfilesFromDetailedDats);
                    //sim.MyGeneralConfig.Enable(CalcOption.OverallSum);
                    Logger.Info("Temperature:" + sim.MyGeneralConfig.SelectedTemperatureProfile);
                    Logger.Info("Geographic:" + sim.MyGeneralConfig.GeographicLocation);
                    sim.Should().NotBeNull();
                    var cmf = new CalcManagerFactory();
                    CalculationProfiler calculationProfiler = new CalculationProfiler();
                    List<CalcOption> options = new List<CalcOption>(); // sim.MyGeneralConfig.AllEnabledOptions();
                    options.Add(CalcOption.DeviceProfilesHouse);
                    options.Add(CalcOption.DeviceProfilesIndividualHouseholds);
                    options.Add(CalcOption.HouseSumProfilesFromDetailedDats);
                    options.Add(CalcOption.SumProfileExternalEntireHouse);
                    options.Add(CalcOption.SumProfileExternalIndividualHouseholds);
                    options.Add(CalcOption.OverallSum);
                    options.Add(CalcOption.DeviceProfileExternalEntireHouse);
                    options.Add(CalcOption.PolysunImportFiles);
                    CalcStartParameterSet csps = new CalcStartParameterSet(sim.GeographicLocations[0],
                        sim.TemperatureProfiles[0], sim.ModularHouseholds[0], EnergyIntensityType.Random,
                        false, null, LoadTypePriority.All, null, null, null, options,
                        new DateTime(2013, 1, 1), new DateTime(2013, 1, 2), new TimeSpan(0, 1, 0), ";", 5, new TimeSpan(0, 10, 0), false, false, 3, 3, calculationProfiler,
                        wd1.WorkingDirectory, false, false, decimalSep, false);
                    var cm = cmf.GetCalcManager(sim, csps, false);
                    cm.Run(ReportCancelFunc);
                    Logger.ImportantInfo("Duration:" + (DateTime.Now - start).TotalSeconds + " seconds");
                    var pathdp = Path.Combine(wd1.WorkingDirectory,
                        DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "DeviceProfiles.Electricity.csv");
                    NumberFormatInfo nfi = new NumberFormatInfo();
                    nfi.NumberDecimalSeparator = decimalSep;

                    double sumDeviceProfiles = 0;
                    using (var sr = new StreamReader(pathdp))
                    {
                        sr.ReadLine(); // header
                        while (!sr.EndOfStream)
                        {
                            var s = sr.ReadLine();
                            if (s == null)
                            {
                                throw new LPGException("The file " + pathdp + " was broken");
                            }
                            var arr = s.Split(';');

                            for (var i = 2; i < arr.Length; i++)
                            {
                                if (arr[i].Length > 0)
                                {
                                    sumDeviceProfiles += myParse(arr[i], nfi);
                                }
                            }
                        }
                    }

                    sumDeviceProfiles.Should().BeGreaterThan(0);
                    Logger.Info("SumDeviceProfiles: " + sumDeviceProfiles);
                    var pathSumProfiles = Path.Combine(wd1.WorkingDirectory,
                        DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "SumProfiles.Electricity.csv");
                    double sumSumProfiles = 0;
                    using (var sr = new StreamReader(pathSumProfiles))
                    {
                        sr.ReadLine(); // header
                        while (!sr.EndOfStream)
                        {
                            var s = sr.ReadLine();
                            if (s != null)
                            {
                                var arr = s.Split(';');

                                for (var i = 2; i < arr.Length; i++)
                                {
                                    if (arr[i].Length > 0)
                                    {
                                        sumSumProfiles += myParse(arr[i], nfi);
                                    }
                                }
                            }
                        }
                    }
                    sumDeviceProfiles.Should().BeApproximatelyWithinPercent(sumSumProfiles,0.001,"path: \n" + pathSumProfiles + "\n" + sumSumProfiles +" vs. \n" + pathdp + "\n" + sumDeviceProfiles);
                    Logger.Info("sumSumProfiles: " + sumSumProfiles);
                    var pathExtSumProfiles = Path.Combine(wd1.WorkingDirectory,
                        DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "SumProfiles_600s.Electricity.csv");
                    double sumExtSumProfiles = 0;
                    using (var sr = new StreamReader(pathExtSumProfiles))
                    {
                        sr.ReadLine(); // header
                        while (!sr.EndOfStream)
                        {
                            var s = sr.ReadLine();
                            if (s != null)
                            {
                                var arr = s.Split(';');

                                for (var i = 2; i < arr.Length; i++)
                                {
                                    if (arr[i].Length > 0)
                                    {
                                        sumExtSumProfiles += myParse(arr[i], nfi);
                                    }
                                }
                            }
                        }
                    }
                    Logger.Info("sumExtSumProfiles: " + sumExtSumProfiles);
                    sumDeviceProfiles.Should().BeApproximatelyWithinPercent(sumExtSumProfiles,0.001);

                    var pathExtDeviceProfiles = Path.Combine(wd1.WorkingDirectory,
                        DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results),
                        "DeviceProfiles_600s.Electricity.csv");
                    double sumExtDeviceProfiles = 0;
                    using (var sr = new StreamReader(pathExtDeviceProfiles))
                    {
                        sr.ReadLine(); // header
                        while (!sr.EndOfStream)
                        {
                            var s = sr.ReadLine();
                            if (s != null)
                            {
                                var arr = s.Split(';');

                                for (var i = 2; i < arr.Length; i++)
                                {
                                    if (arr[i].Length > 0)
                                    {
                                        sumExtDeviceProfiles += myParse(arr[i], nfi);
                                    }
                                }
                            }
                        }
                    }
                    Logger.Info("sumExtDeviceProfiles: " + sumExtDeviceProfiles);
                    sumDeviceProfiles.Should().BeApproximatelyWithinPercent(sumExtSumProfiles, 0.001);
                    var pathImportFile = Path.Combine(wd1.WorkingDirectory,
                        DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "ImportProfile.900s.Electricity.csv");
                    double sumImportFile = 0;
                    using (var sr = new StreamReader(pathImportFile))
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            sr.ReadLine(); // header
                        }

                        while (!sr.EndOfStream)
                        {
                            var s = sr.ReadLine();
                            if (s != null)
                            {
                                var arr = s.Split(';');
                                if (arr.Length > 1 && arr[1].Length > 0)
                                {
                                    var d = double.Parse(arr[1], CultureInfo.InvariantCulture);
                                    sumImportFile += d;
                                }
                            }
                        }
                    }
                    Logger.Info("sumImportFile: " + sumImportFile);
                    var supposedValue = sumDeviceProfiles; // convert to watt/5 min
                    Logger.Info("supposedValue: " + supposedValue);
                    supposedValue.Should().BeApproximatelyWithinPercent(sumImportFile,0.001);

                    var pathExtSumProfileshh1 = Path.Combine(wd1.WorkingDirectory,
                        DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "SumProfiles_600s.HH1.Electricity.csv");
                    double sumExtSumProfilesHH1 = 0;
                    using (var sr = new StreamReader(pathExtSumProfileshh1))
                    {
                        sr.ReadLine(); // header
                        while (!sr.EndOfStream)
                        {
                            var s = sr.ReadLine();
                            if (s != null)
                            {
                                var arr = s.Split(';');

                                for (var i = 2; i < arr.Length; i++)
                                {
                                    if (arr[i].Length > 0)
                                    {
                                        sumExtSumProfilesHH1 += myParse(arr[i], nfi);
                                    }
                                }
                            }
                        }
                    }
                    Logger.Info("sumExtSumProfiles.hh1: " + sumExtSumProfilesHH1);
                    sumDeviceProfiles.Should().BeApproximatelyWithinPercent(sumExtSumProfilesHH1,0.001, "path: \n" + pathExtSumProfileshh1 + "\n" + sumExtSumProfilesHH1 + " vs. \n" + pathdp + "\n" + sumDeviceProfiles);

                    var pathOverallSum = Path.Combine(wd1.WorkingDirectory,
                        DirectoryNames.CalculateTargetdirectory(TargetDirectory.Results), "Overall.SumProfiles.Electricity.csv");
                    double overallSum = 0;
                    using (var sr = new StreamReader(pathOverallSum))
                    {
                        sr.ReadLine(); // header
                        while (!sr.EndOfStream)
                        {
                            var s = sr.ReadLine();
                            if (s != null)
                            {
                                var arr = s.Split(';');

                                for (var i = 2; i < arr.Length; i++)
                                {
                                    if (arr[i].Length > 0) {
                                        var d = myParse(arr[i], nfi); // double.Parse(arr[i], CultureInfo.InvariantCulture);
                                        overallSum += d;
                                    }
                                }
                            }
                        }
                    }
                    Logger.Info("overallSum: " + overallSum);
                    sumDeviceProfiles.Should().BeApproximatelyWithinPercent(overallSum,0.001, "path: \n" + pathdp + "\n" + sumDeviceProfiles + " vs. \n" + pathOverallSum + "\n" + overallSum);

                    CleanTestBase.RunAutomatically(true);
                }
            }
        }

        private static bool ReportCancelFunc()
        {
            Logger.Info("canceled");
            return true;
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunOnlyDevice() {
            RunTest(x => x.Enable(CalcOption.DeviceProfilesIndividualHouseholds), GetCurrentMethod());
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void RunOnlyDeviceProfileExternal() {
            RunTest(x => x.Enable(CalcOption.DeviceProfileExternalEntireHouse), GetCurrentMethod());
        }

        public PostProcessingTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}