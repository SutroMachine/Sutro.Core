using g3;
using Sutro.Core.Models;
using System;
using System.Collections.Generic;

namespace gs
{
    /// <summary>
    /// Represents set of print meshes and per-mesh options
    /// [TODO] this could be more useful...also we might want to include more than just meshes?
    /// </summary>
    public class PrintMeshAssembly
    {
        private class MeshInfo
        {
            public DMesh3 Mesh;
            public PrintMeshOptions Options;
        }

        private List<MeshInfo> meshes = new List<MeshInfo>();

        public IReadOnlyList<DMesh3> Meshes
        {
            get
            {
                List<DMesh3> m = new List<DMesh3>();
                foreach (var mi in meshes)
                    m.Add(mi.Mesh);
                return m;
            }
        }

        public IEnumerable<Tuple<DMesh3, PrintMeshOptions>> MeshesAndOptions()
        {
            foreach (var mi in meshes)
                yield return new Tuple<DMesh3, PrintMeshOptions>(mi.Mesh, mi.Options);
        }

        public void AddMesh(DMesh3 mesh, PrintMeshOptions options)
        {
            MeshInfo mi = new MeshInfo()
            {
                Mesh = mesh,
                Options = options
            };
            meshes.Add(mi);
        }

        public void AddMesh(DMesh3 mesh)
        {
            AddMesh(mesh, PrintMeshOptionsFactory.Default());
        }

        public void AddMeshes(IEnumerable<DMesh3> meshes)
        {
            AddMeshes(meshes, PrintMeshOptionsFactory.Default());
        }

        public void AddMeshes(IEnumerable<DMesh3> meshes, PrintMeshOptions options)
        {
            foreach (var v in meshes)
                AddMesh(v, options);
        }

        public AxisAlignedBox3d TotalBounds
        {
            get
            {
                AxisAlignedBox3d bounds = AxisAlignedBox3d.Empty;
                foreach (var mesh in Meshes)
                    bounds.Contain(mesh.CachedBounds);
                return bounds;
            }
        }
    }
}