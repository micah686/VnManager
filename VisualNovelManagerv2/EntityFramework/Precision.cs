using System;
using System.Data.Entity;
using System.Linq;

namespace VisualNovelManagerv2.EntityFramework
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class Precision : Attribute
    {
        /// <summary>
        /// The total number of digits to store, including decimals
        /// </summary>
        public byte precision { get; set; }
        /// <summary>
        /// The number of digits from the precision to be used for decimals
        /// </summary>
        public byte scale { get; set; }

        /// <summary>
        /// Define the precision and scale of a decimal data type
        /// </summary>
        /// <param name="precision">The total number of digits to store, including decimals</param>
        /// <param name="scale">The number of digits from the precision to be used for decimals</param>
        public Precision(byte precision, byte scale)
        {
            this.precision = precision;
            this.scale = scale;
        }

        /// <summary>
        /// Apply the precision to our data model for any property using this annotation
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void ConfigureModelBuilder(DbModelBuilder modelBuilder)
        {
            modelBuilder.Properties().Where(x => x.GetCustomAttributes(false).OfType<Precision>().Any())
                .Configure(c => c.HasPrecision(c.ClrPropertyInfo.GetCustomAttributes(false).OfType<Precision>().First()
                    .precision, c.ClrPropertyInfo.GetCustomAttributes(false).OfType<Precision>().First().scale));
        }
    }
}
