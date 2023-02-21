using System;
using System.Collections.Generic;
using SQLite;

namespace CompassMobileUpdate.Models
{
	public class LocalVoltageRule : VoltageRule
	{
        [PrimaryKey]
        public int Id { get; set; }

        [Indexed]
        public DateTime CreatedTime { get; set; }

        public void ConvertVoltageRuleToLocalVoltageRule(VoltageRule vr)
        {
            this.ID = vr.ID;
            this.CommercialVoltageHigh = vr.CommercialVoltageHigh;
            this.CommercialVoltageLow = vr.CommercialVoltageLow;
            this.MeterForm = vr.MeterForm;
            this.MeterType = vr.MeterType;
            this.ResidentialVoltageHigh = vr.ResidentialVoltageHigh;
            this.ResidentialVoltageLow = vr.ResidentialVoltageLow;
            this.TargetVoltage = vr.TargetVoltage;
            this.OverlappingVoltages = vr.OverlappingVoltages;
        }

        public static LocalVoltageRule GetLocalVoltageRuleFromVoltageRule(VoltageRule vr)
        {
            LocalVoltageRule local = new LocalVoltageRule();
            local.ConvertVoltageRuleToLocalVoltageRule(vr);
            return local;
        }

        public static List<LocalVoltageRule> GetListOfLocalVoltageRulesFromListOfVoltageRules(List<VoltageRule> vrs)
        {
            List<LocalVoltageRule> lvrs = new List<LocalVoltageRule>();

            for (int i = 0; i < vrs.Count; i++)
            {
                lvrs.Add(GetLocalVoltageRuleFromVoltageRule(vrs[i]));
            }

            return lvrs;
        }
    }
}

