//
// Unit tests for AvoidRepetitiveCastsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Performance;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Performance {

	[TestFixture]
	public class AvoidRepetitiveCastsTest : MethodRuleTestFixture<AvoidRepetitiveCastsRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		private void UnneededCast_IsInst (ICollection list)
		{
			foreach (object o in list) {
				if (o is ICollection) {
					UnneededCast_IsInst ((ICollection) o);
				}
			}
		}

		private void UnneededCast_CastClass (ICollection list)
		{
			foreach (object o in list) {
				if (o is ICollection) {
					UnneededCast_CastClass (o as ICollection);
				}
			}
		}

		private void SingleCast (ICollection list)
		{
			foreach (object o in list) {
				ICollection c = (o as ICollection);
				if (c != null) {
					SingleCast (c);
				}
			}
		}

		[Test]
		public void AvoidUnneededCast ()
		{
			AssertRuleFailure<AvoidRepetitiveCastsTest> ("UnneededCast_IsInst", 1);
			AssertRuleFailure<AvoidRepetitiveCastsTest> ("UnneededCast_CastClass", 1);
			AssertRuleSuccess<AvoidRepetitiveCastsTest> ("SingleCast");
		}

		private void DifferentCastType (ICollection list)
		{
			foreach (object o in list) {
				if (o is IEnumerable) {
					DifferentCastType (o as ICollection);
				}
			}
		}

		[Test]
		public void DifferentCast ()
		{
			AssertRuleSuccess<AvoidRepetitiveCastsTest> ("DifferentCastType");
		}

		private void ArrayCast (object [] list)
		{
			foreach (object o in list) {
				if (o is object[]) {
					ArrayCast (o as object []);
				}
			}
		}

		private void ArrayElementCast (object [] list)
		{
			if (list [0] is string) {
				Console.WriteLine (list [0] as string);
			}
		}

		private void ArrayElementAsArrayCast (object [] list, int index)
		{
			if (list [index * 2 + 1] is object[]) {
				ArrayElementAsArrayCast (list [index * 2 + 1] as object [], index);
			}
		}

		private void ArrayGood (object [] list)
		{
			foreach (object o in list) {
				object[] array = (o as object []);
				if (array != null)
					ArrayCast (array);
			}
			string s = (list [0] as string);
			if (s != null)
				Console.WriteLine (s);
		}

		private object [] results;
		public string PropertyArrayField {
			get { return ((string) (this.results [0])); }
		}

		[Test]
		public void Arrays ()
		{
			AssertRuleFailure<AvoidRepetitiveCastsTest> ("ArrayCast", 1);
			AssertRuleFailure<AvoidRepetitiveCastsTest> ("ArrayElementCast", 1);
			AssertRuleFailure<AvoidRepetitiveCastsTest> ("ArrayElementAsArrayCast", 1);

			AssertRuleSuccess<AvoidRepetitiveCastsTest> ("ArrayGood");
			AssertRuleSuccess<AvoidRepetitiveCastsTest> ("get_PropertyArrayField");
		}

		private void CheckSelf ()
		{
			if (this is AvoidRepetitiveCastsTest)
				Console.WriteLine ("of course");
		}

		[Test]
		public void This ()
		{
			AssertRuleSuccess<AvoidRepetitiveCastsTest> ("CheckSelf");
		}

		private object GuessWhat ()
		{
			return "string";
		}

		private void ReturnValue ()
		{
			if (GuessWhat () is string)
				Console.WriteLine ("of course");
		}

		private void Foreach (TypeDefinition type)
		{
			foreach (MethodDefinition ctor in type.Constructors) {
				Console.WriteLine (ctor);
			}
			foreach (MethodDefinition method in type.Methods) {
				Console.WriteLine (method);
			}
		}

		[Test]
		public void Calls ()
		{
			AssertRuleSuccess<AvoidRepetitiveCastsTest> ("ReturnValue");
			AssertRuleSuccess<AvoidRepetitiveCastsTest> ("Foreach");
		}

		private ArrayList RefBad (ref IList list)
		{
			if (list is ArrayList) {
				return (list as ArrayList);
			}
			return null;
		}

		private long RefGood (ref int value)
		{
			return (long) value; // actually it's a convertion not a cast
		}

		private bool Out (IList list, out IList al)
		{
			al = list;
			return (al is ArrayList);
		}

		[Test]
		public void Arguments ()
		{
			AssertRuleFailure<AvoidRepetitiveCastsTest> ("RefBad", 1);
			AssertRuleSuccess<AvoidRepetitiveCastsTest> ("RefGood");
			AssertRuleSuccess<AvoidRepetitiveCastsTest> ("Out");
		}

		// from mcs/class/System.Web/System.Web/CapabilitiesLoader.cs
		private Hashtable data;
		private string GetParentName ()
		{
			return (string) (data.Contains ("parent") ? data ["parent"] : null);
		}

		[Test]
		public void Null ()
		{
			AssertRuleSuccess<AvoidRepetitiveCastsTest> ("GetParentName");
		}
	}
}