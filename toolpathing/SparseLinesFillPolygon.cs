﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;

namespace gs
{
    /// <summary>
    /// configure dense-fill for sparse infill
    /// </summary>
    public class SparseLinesFillPolygon : ParallelLinesFillPolygon
    {
        public SparseLinesFillPolygon(GeneralPolygon2d poly) : base(poly)
        {
            SimplifyAmount = SimplificationLevel.Moderate;
            TypeFlags = PathTypeFlags.SparseInfill;
        }
    }



    /// <summary>
    /// configure dense-fill for support fill
    /// </summary>
    public class SupportLinesFillPolygon : ParallelLinesFillPolygon
    {
        public SupportLinesFillPolygon(GeneralPolygon2d poly) : base(poly)
        {
            SimplifyAmount = SimplificationLevel.Aggressive;
            TypeFlags = PathTypeFlags.SupportMaterial;
        }
    }

}
