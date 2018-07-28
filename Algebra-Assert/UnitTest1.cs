using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedAlgebra.NET;

namespace Algebra_Assert
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestDihedralGroup()
        {
            // Symmetry group of an octogon
            DihedralGroup G = new DihedralGroup(8);

            // Make sure all elements have a suitable inverse and are unchanged by the identity under the operation
            for (int i = 0; i < G.PolySize; i++)
            {
                DihedralGroupElement x = G.Spawn(Transformation.Rotation, i);
                Assert.AreEqual(G.Identity, G.Operation(x, G.Inverse(x)));
                Assert.AreEqual(G.Operation(G.Identity, x), x);
            }

            // Ensure a reflection is counted as its own inverse
            Assert.AreEqual(G.Spawn(Transformation.Reflection), G.Inverse(G.Spawn(Transformation.Reflection)));

            //Ensure the identity is its own inverse
            Assert.AreEqual(G.Identity, G.Inverse(G.Identity));
        }

        [TestMethod]
        public void TestModularGroup()
        {
            // G = Z10
            ModularGroup G = new ModularGroup(10);

            // Make sure every element has an inverse, and is unchanged by the identity under the operation
            for (int i = 0; i < G.Modulus; i++)
            {
                Assert.AreEqual(G.Identity, G.Operation(i, G.Inverse(i)));
                Assert.AreEqual(G.Operation(G.Identity, i), i);
            }

            ModularMonoid M = G;
            ModularRing R = G;

            // Ensure casts are working properly
            Assert.AreEqual(M, (ModularMonoid)R);
            Assert.AreEqual(M, (ModularMonoid)G);
            Assert.AreEqual(R, (ModularRing)G);
            Assert.AreEqual(R, (ModularRing)M);
            Assert.AreEqual(G, (ModularGroup)R);
            Assert.AreEqual(G, (ModularGroup)M);
        }

        [TestMethod]
        public void TestModularRing()
        {
            ModularRing R = new ModularRing(12);

            for (int i = 0; i < R.Modulus; i++)
            {
                Assert.AreEqual(R.Add(i, R.AdditiveStructure.Inverse(i)), R.AdditiveIdentity);
                Assert.AreEqual(R.Add(i, R.AdditiveIdentity), i);

                // While modular rings may have multiplicative inverses, it is only the case if the modulus is prime
                // We can't ask for a ring to have this -> included in Field interface
                Assert.AreEqual(R.Multiply(i, R.MultiplicativeIdentity), i);
            }
        }
    }
}
