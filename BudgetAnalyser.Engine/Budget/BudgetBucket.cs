﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    [DebuggerDisplay("{TypeDescription} {Code} {Description}")]
    public abstract class BudgetBucket : IModelValidate, INotifyPropertyChanged, IComparable
    {
        private bool doNotUseActive;
        private string doNotUseCode;
        private string doNotUseDescription;

        protected BudgetBucket()
        {
            this.doNotUseActive = true;
        }

        protected BudgetBucket(string code, string name) : this()
        {
            if (code == null)
            {
                throw new ArgumentNullException("code");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.doNotUseDescription = name;
            this.doNotUseCode = code.ToUpperInvariant();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="BudgetBucket" /> is active.
        ///     If Inactive the Bucket will not be used in auto-matching nor available for manual transation matching.
        /// </summary>
        /// <value>
        ///     <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        public bool Active
        {
            get { return this.doNotUseActive; }
            set
            {
                this.doNotUseActive = value;
                OnPropertyChanged();
            }
        }

        public string Code
        {
            get { return this.doNotUseCode; }

            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                this.doNotUseCode = value.ToUpperInvariant();
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return this.doNotUseDescription; }
            set
            {
                this.doNotUseDescription = value;
                OnPropertyChanged();
            }
        }

        public virtual string TypeDescription
        {
            get { return GetType().Name; }
        }

        public int CompareTo(object obj)
        {
            var otherBucket = obj as BudgetBucket;
            if (otherBucket == null)
            {
                return -1;
            }

            return string.Compare(Code, otherBucket.Code, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            var otherBucket = obj as BudgetBucket;
            if (otherBucket == null)
            {
                return false;
            }

            return Code == otherBucket.Code;
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "[{0}] {1}", Code, Description);
        }

        public bool Validate([NotNull] StringBuilder validationMessages)
        {
            if (validationMessages == null)
            {
                throw new ArgumentNullException("validationMessages");
            }

            var retval = true;
            if (string.IsNullOrWhiteSpace(Code))
            {
                validationMessages.AppendFormat("Budget Bucket {0} is invalid, Code must be a small textual code.", Code);
                retval = false;
            }
            else
            {
                if (Code.Length > 7)
                {
                    validationMessages.AppendFormat("Budget Bucket {0} - {1} is invalid, Code must be a small textual code less than 7 characters.", Code, Description);
                    retval = false;
                }
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                validationMessages.AppendFormat("Budget Bucket {0} is invalid, Description must not be blank.", Code);
                retval = false;
            }

            return retval;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static bool operator ==(BudgetBucket obj1, BudgetBucket obj2)
        {
            object obj3 = obj1, obj4 = obj2;
            if (obj3 == null && obj4 == null)
            {
                return true;
            }

            if (obj3 == null || obj4 == null)
            {
                return false;
            }

            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator >([NotNull] BudgetBucket obj1, [NotNull] BudgetBucket obj2)
        {
            if (obj1 == null)
            {
                throw new ArgumentNullException("obj1");
            }

            if (obj2 == null)
            {
                throw new ArgumentNullException("obj2");
            }

            return obj1.CompareTo(obj2) > 0;
        }

        public static bool operator !=(BudgetBucket obj1, BudgetBucket obj2)
        {
            return !(obj1 == obj2);
        }

        public static bool operator <([NotNull] BudgetBucket obj1, [NotNull] BudgetBucket obj2)
        {
            if (obj1 == null)
            {
                throw new ArgumentNullException("obj1");
            }

            if (obj2 == null)
            {
                throw new ArgumentNullException("obj2");
            }

            return obj1.CompareTo(obj2) < 0;
        }
    }
}