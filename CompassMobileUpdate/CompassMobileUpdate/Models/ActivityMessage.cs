using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace CompassMobileUpdate.Models
{
    public class ActivityMessage
    {
        //For ActivityRequest and it's Derived classes I've implemented the DataContract Attribute
        //because otherwise it would serialize ActionName and RequestType in all service calls and we don't need that
        [DataContract]
        public class ActivityRequest
        {
            [IgnoreDataMember]
            public string ActionName { get; protected set; }
            [IgnoreDataMember]
            public ActivityRequestType RequestType { get; protected set; }
            public virtual Object GetBody()
            {
                return this;
            }

        }
        public class ActivityResponse
        {
            public int Value { get; set; }
        }
        [DataContract]
        public class PostActivityCompleteRequest
        {
            [DataMember]
            public int ActivityID { get; set; }

        }
        [DataContract]
        public class PostActivityComplete : ActivityRequest
        {
            [DataMember]
            public int ActivityID { get; set; }
            public override Object GetBody()
            {
                return this;
            }

            public PostActivityComplete()
            {
                this.ActionName = ActionNames.PostActivityComplete;
                this.RequestType = ActivityRequestType.PostActivityComplete;
            }
        }

        [DataContract]
        public class PostMeterPingActivityCompleteRequest : ActivityRequest
        {
            [DataMember]
            public int ActivityID { get; set; }
            [DataMember]
            public string MeterDeviceUtilityID { get; set; }
            [DataMember]
            public ResultEnum Result { get; set; }
            [DataMember]
            public StatusEnum Status { get; set; }

            public override Object GetBody()
            {
                return this;
            }
            public PostMeterPingActivityCompleteRequest()
            {
                this.ActionName = ActionNames.PostMeterPingActivityComplete;
                this.RequestType = ActivityRequestType.PostMeterPingActivityComplete;
            }

        }// end PostMeterPingActivityCompleteRequest

        [DataContract]
        public class LogMeterPQRActivityRequest : ActivityRequest
        {
            [DataMember]
            public int ActivityID { get; set; }
            [DataMember]
            public string MeterDeviceUtilityID { get; set; }
            public override Object GetBody()
            {
                return this;
            }
            public LogMeterPQRActivityRequest()
            {
                this.ActionName = ActionNames.LogMeterPQRActivity;
                this.RequestType = ActivityRequestType.LogMeterPQRActivity;
            }
        }//end LogMeterPQRActivityRequest

        [DataContract]
        public class PostPQRActivityCompleteRequest : ActivityRequest
        {
            [DataMember]
            public int ActivityID { get; set; }
            [DataMember]
            public string MeterDeviceUtilityID { get; set; }
            [DataMember]
            public decimal? VoltagePhaseA { get; set; }
            [DataMember]
            public bool? IsVoltagePhaseAInRange { get; set; }
            [DataMember]
            public decimal? VoltagePhaseB { get; set; }
            [DataMember]
            public bool? IsVoltagePhaseBInRange { get; set; }
            [DataMember]
            public decimal? VoltagePhaseC { get; set; }
            [DataMember]
            public bool? IsVoltagePhaseCInRange { get; set; }
            [DataMember]
            public StatusEnum Status { get; set; }
            [DataMember]
            public ResultEnum Result { get; set; }

            public override Object GetBody()
            {
                return this;
            }
            public PostPQRActivityCompleteRequest()
            {
                this.ActionName = ActionNames.PostPQRActivityComplete;
                this.RequestType = ActivityRequestType.PostPQRActivityComplete;
            }
            public ResultEnum GetActivityPQRResult()
            {
                bool nullA, nullB, nullC;
                nullA = nullB = nullC = false;

                if (this.VoltagePhaseA.HasValue)
                {
                    if (this.IsVoltagePhaseAInRange.HasValue && !this.IsVoltagePhaseAInRange.Value)
                        return ResultEnum.FAILED;
                    else if (this.IsVoltagePhaseAInRange == null)
                    {
                        nullA = true;
                    }
                }
                if (this.VoltagePhaseB.HasValue)
                {
                    if (this.IsVoltagePhaseBInRange.HasValue && !this.IsVoltagePhaseBInRange.Value)
                        return ResultEnum.FAILED;
                    else if (this.IsVoltagePhaseBInRange == null)
                    {
                        nullB = true;
                    }
                }
                if (this.VoltagePhaseC.HasValue)
                {
                    if (this.IsVoltagePhaseCInRange.HasValue && !this.IsVoltagePhaseCInRange.Value)
                        return ResultEnum.FAILED;
                    else if (this.IsVoltagePhaseCInRange == null)
                    {
                        nullC = true;
                    }
                }

                if (nullA || nullB || nullC)
                {
                    return ResultEnum.NULL;
                }

                if (!this.VoltagePhaseA.HasValue && !this.VoltagePhaseB.HasValue && !this.VoltagePhaseC.HasValue)
                {
                    return ResultEnum.NULL;
                }
                else
                {
                    return ResultEnum.OK;
                }

            }
        }//End class POSTPQRActivityCompleteRequest
        public class LogMeterPingActivityRequest
        {
            public int ActivityID { get; set; }
            public string MeterDeviceUtilityID { get; set; }
            public string MeterState { get; set; }
        }
        [DataContract]
        public class LogActivityAndMeterPingActivityRequest : ActivityRequest
        {
            [DataMember]
            public string MeterDeviceUtilityID { get; set; }
            [DataMember]
            public string UserID { get; set; }
            [DataMember]
            public string MeterState { get; set; }

            public override Object GetBody()
            {
                return this;
            }

            public LogActivityAndMeterPingActivityRequest()
            {
                this.ActionName = ActionNames.LogActivityAndMeterPingActivity;
                this.RequestType = ActivityRequestType.LogActivityAndMeterPingActivity;
            }


        }//End LogActivityAndMeterPingActivityRequest
        public class MeterPingActivity
        {
            private Activity _activity;
            public string MeterDeviceUtilityID
            {
                get { return _activity.Tag; }
                set { _activity.Tag = value; }
            }
            public string UserID
            {
                get { return _activity.UserID; }
                set { _activity.UserID = value; }
            }

            public string MeterState { get; set; }
            public MeterPingActivity()
            {
                _activity = new Activity
                {
                    Type = ActivityType.METER
                };
            }

            public Activity GetActivity()
            {
                return _activity;
            }
        }//End MeterPingActivity
    }//End ActivityRequest

    public enum ActivityRequestType
    {
        LogActivityAndMeterPingActivity,
        PostMeterPingActivityComplete,
        LogMeterPQRActivity,
        PostPQRActivityComplete,
        PostActivityComplete
    }

    public enum StatusEnum
    {
        E,
        C,
        I
    }
    public enum ResultEnum
    {
        NULL,
        FAILED,
        OK,
        CANCELLED
    }

    public class ActionNames
    {
        public const string LogActivityAndMeterPingActivity = "LogActivityAndMeterPingActivity";
        public const string LogMeterPQRActivity = "LogMeterPQRActivity";
        public const string PostActivityComplete = "PostActivityComplete";
        public const string PostMeterPingActivityComplete = "PostMeterPingActivityComplete";
        public const string PostPQRActivityComplete = "PostPQRActivityComplete";
    }
}

