using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace RedAlgebra.NET
{
    // Contains interfaces for algebraic structures
    public class Scaffolding
    {
        // Most basic structure, implements only an operation
        public interface IGroupoid<T> {
            T Operation(T a, T b);
        }

        // No 'enforcable' rules applied by the extension ISemiGroup of IGroupoid
        public interface ISemiGroup<T> : IGroupoid<T> {
            // Nothing to add, but including just in case someone needs a semigroup vs a monoid
        }

        // Monoids have an operation, but they also have an identity
        public interface IMonoid<T> : ISemiGroup<T> {
            T Identity { get; }
        }

        // Groups also have an inverse
        public interface IGroup<T> : IMonoid<T> {
            T Inverse(T a);
        }

        // Monoid under addition and multiplication
        public interface ISemiRing<T> {
            IMonoid<T> AdditiveStructure { get; }
            IMonoid<T> MultiplicativeStructure { get; }
        }

        // Abelian group under addition, monoid under multiplication
        public interface IRing<T, TGroup, TMonoid> 
            where TGroup : IGroup<T>
            where TMonoid : IMonoid<T>
        {
            TGroup AdditiveStructure { get; }
            TMonoid MultiplicativeStructure { get; }

            T Add(T r, T s);
            T Multiply(T r, T s);
        }

        // Abelian group under addition and multiplication
        public interface IField<T, TAddGroup, TMulGroup>
            where TAddGroup : IGroup<T>
            where TMulGroup : IGroup<T>
        {
            TAddGroup AdditiveStructure { get; }
            TMulGroup MultiplicativeStructure { get; }
        }
    }

    /// <summary>
    /// In all classes, Equals() is overridden in such a way as to mean "Isomorphic to" and not "Equal to"
    /// </summary>

    // Multiplicative modular monoid
    public class ModularMonoid : Scaffolding.IMonoid<int>
    {
        public int Identity { get; } = 1;
        public int Modulus { get; }

        public int Operation(int a, int b)
        {
            return (a * b) % Modulus;
        }

        // Operators for casting purposes
        public static implicit operator ModularGroup (ModularMonoid x)
        {
            return new ModularGroup(x.Modulus);
        }

        public static implicit operator ModularRing (ModularMonoid x)
        {
            return new ModularRing(x.Modulus);
        }

        public override bool Equals(object obj)
        {
            if (obj is ModularMonoid)
            {
                ModularMonoid that = obj as ModularMonoid;
                return this.Modulus == that.Modulus;
            }

            return false;
        }

        public ModularMonoid(int modulus)
        {
            Modulus = modulus;
        }
    }

    // Modular groups, this is an implementation of Zn for arbitrary n
    public class ModularGroup : Scaffolding.IGroup<int>
    {
        public int Operation(int n, int m)
        {
            return (n + m) % Modulus;
        }

        public int Identity { get; } = 0;

        public int Modulus { get; }

        public int Inverse(int a)
        {
            return Modulus - (a % Modulus);
        }

        // Operators for casting purposes
        public static implicit operator ModularMonoid (ModularGroup x)
        {
            return new ModularMonoid(x.Modulus);
        }

        public static implicit operator ModularRing (ModularGroup x)
        {
            return new ModularRing(x.Modulus);
        }

        public override bool Equals(object obj)
        {
            if (obj is ModularGroup)
            {
                ModularGroup that = obj as ModularGroup;
                return this.Modulus == that.Modulus;
            }

            return false;
        }

        public ModularGroup(int modulus)
        {
            Modulus = modulus;
        }
    }

    // Group of symmetries on an n-gon. PolySize represents the size of that n-gon.
    public class DihedralGroup : Scaffolding.IGroup<DihedralGroupElement>
    {
        public int PolySize { get; }

        public DihedralGroupElement Identity { get; }

        // Returns an element within the context of this group -- quick shorthand to get a particular element
        public DihedralGroupElement Spawn(Transformation t, int? rotationSize = 0)
        {
            if (t == Transformation.Reflection) { rotationSize = PolySize / 2; }
            if (rotationSize == 0) { t = Transformation.Identity; }

            return new DihedralGroupElement(t, this, (rotationSize % PolySize));
        }

        // Gets the inverse of a given element
        public DihedralGroupElement Inverse(DihedralGroupElement g)
        {
            // e and s are their own inverses
            if (g.type == Transformation.Identity || g.type == Transformation.Reflection || g.power == (PolySize / 2)) { return g; }

            else
            {
                return new DihedralGroupElement(Transformation.Rotation, this, (PolySize - (g.power % PolySize)));
            }
        }

        // Multiply two symmetries
        public DihedralGroupElement Operation(DihedralGroupElement g, DihedralGroupElement h)
        {
            Transformation t = new Transformation();
            int newPower = (g.power + h.power) % PolySize;

            if (newPower == 0) { t = Transformation.Identity; }
            else if (newPower == (PolySize / 2)) { t = Transformation.Reflection; }
            else { t = Transformation.Rotation; }

            return new DihedralGroupElement(t, this, newPower);
        }

        public override bool Equals(object obj)
        {
            if (obj is DihedralGroup)
            {
                var that = obj as DihedralGroup;
                return this.PolySize == that.PolySize;
            }

            return false;
        }

        public DihedralGroup(int size)
        {
            PolySize = size;
            Identity = new DihedralGroupElement(Transformation.Identity, this);
        }

    }

    // Element of a dihedral group
    public class DihedralGroupElement
    {
        // All elements expressible as r^k for k < PolySize, hence we only really care about k
        public int power { get; set; }
        public Transformation type { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is DihedralGroupElement)
            {
                var that = obj as DihedralGroupElement;
                return this.power == that.power;
            }

            return false;
        }

        // Constructor -- passed a group G to get some properties. This is for internal use
        // To generate elements in the context of a group, use DihedralGroup.Spawn()
        public DihedralGroupElement(Transformation elementType, DihedralGroup G, int? rotationSize = null)
        {
            type = elementType;
            if (type == Transformation.Identity) { power = 0; }
            else if (type == Transformation.Reflection) { power = (G.PolySize / 2); }
            else
            {
                if (!rotationSize.HasValue)
                {
                    power = 1;
                }

                else { power = (int)rotationSize % G.PolySize; }
            }
        }
    }

    // Z mod n viewed w/ a ring structure
    public class ModularRing : Scaffolding.IRing<int, ModularGroup, ModularMonoid>
    {
        // The monoid and group will have the same modulus
        public ModularGroup AdditiveStructure { get; }
        public ModularMonoid MultiplicativeStructure { get; }

        // Set in constructor so user doesn't have to access Ring.typeofStructure.Modulus
        public int Modulus { get; }

        // Similar reasoning to the above
        public int AdditiveIdentity { get; }
        public int MultiplicativeIdentity { get; }

        // Shortcut to modular addition
        public int Add(int m, int n)
        {
            return AdditiveStructure.Operation(m, n);
        }

        // Shortcut to modular multiplication
        public int Multiply(int m, int n)
        {
            return MultiplicativeStructure.Operation(m, n);
        }

        public static implicit operator ModularMonoid (ModularRing x)
        {
            return new ModularMonoid(x.MultiplicativeStructure.Modulus);
        }

        public static implicit operator ModularGroup (ModularRing x)
        {
            return new ModularGroup(x.AdditiveStructure.Modulus);
        }

        public override bool Equals(object obj)
        {
            if (obj is ModularRing)
            {
                ModularRing that = obj as ModularRing;
                return that.AdditiveStructure.Equals(this.AdditiveStructure)
                    && that.MultiplicativeStructure.Equals(this.MultiplicativeStructure);
            }

            return false;
        }

        public ModularRing(int modulus)
        {
            AdditiveStructure = new ModularGroup(modulus);
            MultiplicativeStructure = new ModularMonoid(modulus);

            AdditiveIdentity = AdditiveStructure.Identity;
            MultiplicativeIdentity = MultiplicativeStructure.Identity;

            Modulus = modulus;
        }

    }

    // Elements are rotations, reflections, or the identity.
    public enum Transformation
    {
        Rotation,
        Reflection,
        Identity
    }
}
