﻿using Automation;
using Automation.ResultFiles;

namespace Common.SQLResultLogging.Loggers {
    using System.Collections.Generic;
    using System.Linq;
    using SQLResultLogging;

    public class TransportationStatusLogger : DataSaverBase {
        private const string TableName = "TransportationStatuses";

        public TransportationStatusLogger([JetBrains.Annotations.NotNull] SqlResultLoggingService srls) : base(typeof(TransportationStatus),
            new ResultTableDefinition(TableName, ResultTableID.TransportationStatuses, "Transportation Status Messages", CalcOption.TransportationStatistics ),
            srls)
        {
        }

        public override void Run(HouseholdKey key, object o)
        {
            var objects = (List<IHouseholdKey>)o;
            var actionEntries = objects.ConvertAll(x => (TransportationStatus)x).ToList();
            SaveableEntry se = new SaveableEntry(key, ResultTableDefinition);
            se.AddField("TimeStep", SqliteDataType.Integer);
            se.AddField("Message", SqliteDataType.Text);
            foreach (var actionEntry in actionEntries) {
                se.AddRow(RowBuilder.Start("Message", actionEntry.StatusMessage).Add("TimeStep", actionEntry.Timestep).ToDictionary());
            }

            if (Srls == null) {
                throw new LPGException("Data Logger was null.");
            }

            Srls.SaveResultEntry(se);
        }

        /*
        [ItemNotNull]
        [JetBrains.Annotations.NotNull]
        public List<ActionEntry> Read([JetBrains.Annotations.NotNull]HouseholdKey hhkey)
        {
            var res = _Srls.ReadFromJson<ActionEntry>(TableName, hhkey,ExpectedResultCount.OneOrMore);
            return res;
        }*/
    }
}