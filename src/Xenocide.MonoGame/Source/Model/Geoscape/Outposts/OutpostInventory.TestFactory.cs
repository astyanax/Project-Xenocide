using ProjectXenocide.Model.Geoscape;

namespace ProjectXenocide.Model.Geoscape.Outposts
{
    /// <summary>
    /// Test-only factory helpers for OutpostInventory.
    /// </summary>
    /// <remarks>
    /// Kept out of the <c>UnitTest*.cs</c> sources, which the project strips from
    /// Release builds, so the xUnit test project still compiles (and runs this
    /// factory) when the whole solution is built with <c>-c Release</c>.
    /// </remarks>
    public partial class OutpostInventory
    {
        /// <summary>
        /// Construct a base with storage facilities, for testing purposes
        /// </summary>
        /// <returns>the constructed base</returns>
        public static Outpost ConstructTestOutpost()
        {
            Outpost outpost = new Outpost(new GeoPosition(), "testOutpost");

            // layout is this (because we must have an access lift)
            //  +------+------+------+------+
            //  |             |             |
            //  +    Pad 1    +     Pad 2   +
            //  |             |             |
            //  +------+------+------+------+
            //  | Lift | Store|
            //  +------+------+
            //  |Baraks| Xeno |
            //  +------+------+
            //  | Lab  |
            //  +------+

            FacilityHandle lift = new FacilityHandle("FAC_BASE_ACCESS_FACILITY", 0, 2);
            outpost.Floorplan.AddFacility(lift);
            FacilityHandle store1 = new FacilityHandle("FAC_STORAGE_FACILITY", 1, 2);
            outpost.Floorplan.AddFacility(store1);
            FacilityHandle Pad1 = new FacilityHandle("FAC_LANDING_PAD", 0, 0);
            outpost.Floorplan.AddFacility(Pad1);
            FacilityHandle Pad2 = new FacilityHandle("FAC_LANDING_PAD", 2, 0);
            outpost.Floorplan.AddFacility(Pad2);
            FacilityHandle barack = new FacilityHandle("FAC_BARRACKS_FACILITY", 0, 3);
            outpost.Floorplan.AddFacility(barack);
            FacilityHandle xenoContainment = new FacilityHandle("FAC_XENOMORPH_HOLDING_FACILITY", 1, 3);
            outpost.Floorplan.AddFacility(xenoContainment);
            FacilityHandle lab = new FacilityHandle("FAC_RESEARCH_FACILITY", 0, 4);
            outpost.Floorplan.AddFacility(lab);

            return outpost;
        }
    }
}
