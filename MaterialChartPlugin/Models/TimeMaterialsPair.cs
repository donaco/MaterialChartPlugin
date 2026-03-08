using System;
using System.Runtime.Serialization;

namespace MaterialChartPlugin.Models
{
    [DataContract]
    public class TimeMaterialsPair : IEquatable<TimeMaterialsPair>
    {
        /// <summary>
        /// 時刻
        /// </summary>
        [DataMember(Order = 1)]
        public DateTime DateTime { get; private set; }

        /// <summary>
        /// 燃料
        /// </summary>
        [DataMember(Order = 2)]
        public int Fuel { get; private set; }

        /// <summary>
        /// 弾薬
        /// </summary>
        [DataMember(Order = 3)]
        public int Ammunition { get; private set; }

        /// <summary>
        /// 鋼材
        /// </summary>
        [DataMember(Order = 4)]
        public int Steel { get; private set; }

        /// <summary>
        /// ボーキサイト
        /// </summary>
        [DataMember(Order = 5)]
        public int Bauxite { get; private set; }

        /// <summary>
        /// 高速修復材
        /// </summary>
        [DataMember(Order = 6)]
        public int RepairTool { get; private set; }

        /// <summary>
        /// 開発資材
        /// </summary>
        [DataMember(Order = 7)]
        public int DevelopmentTool { get; private set; }

        /// <summary>
        /// 高速建造材
        /// </summary>
        [DataMember(Order = 8)]
        public int InstantBuildTool { get; private set; }

        /// <summary>
        /// 回収資材
        /// </summary>
        [DataMember(Order = 9)]
        public int ImprovementTool { get; private set; }

        public int MostMaterial => Math.Max(Math.Max(Math.Max(Fuel, Ammunition), Steel), Bauxite);

        public TimeMaterialsPair() { }

        public TimeMaterialsPair(DateTime dateTime, int fuel, int ammunition, int steel, int bauxite, int repairTool,
            int developmentTool, int instantBuildTool, int improvementTool)
        {
            this.DateTime = dateTime;
            this.Fuel = fuel;
            this.Ammunition = ammunition;
            this.Steel = steel;
            this.Bauxite = bauxite;
            this.RepairTool = repairTool;

            // いつか使うかも？
            this.DevelopmentTool = developmentTool;
            this.InstantBuildTool = instantBuildTool;
            this.ImprovementTool = improvementTool;
        }

        public bool Equals(TimeMaterialsPair other)
        {
            return this.DateTime == other.DateTime && this.Fuel == other.Fuel && this.Ammunition == other.Ammunition
                && this.Steel == other.Steel && this.Bauxite == other.Bauxite && this.RepairTool == other.RepairTool;
        }

        public override string ToString()
        {
            return $"DateTime={DateTime}, Fuel={Fuel}, Ammunition={Ammunition}, Steel={Steel}, Bauxite={Bauxite}, RepairTool={RepairTool}, DevelopmentTool={DevelopmentTool}, InstantBuildTool={InstantBuildTool}, ImprovementTool={ImprovementTool}";
        }
    }
}
