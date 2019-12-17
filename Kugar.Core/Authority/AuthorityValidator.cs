using System;
using System.Collections.Generic;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Authority
{
    public interface IAuthorityValidator
    {
        bool IsContainsAll(params string[] codes);
    }

    /// <summary>
    /// 用于校验权限的类
    /// </summary>
    public class AuthorityValidator : IAuthorityValidator
    {
        private static AuthorityValidator _default=new AuthorityValidator(null);

        private HashSet<string> _hash = null;

        public AuthorityValidator(string[] codes)
        {
            if (codes.HasData())
            {
                _hash = new HashSet<string>(codes, StringComparer.CurrentCultureIgnoreCase);
            }
            else
            {
                _hash=new HashSet<string>();
            }
        }

        public virtual bool IsContainsAll(params string[] codes)
        {
            if (!_hash.HasData())
            {
                return false;
            }

            foreach (var code in codes)
            {
                if (!_hash.Contains(code))
                {
                    return false;
                }
            }

            return true;
        }

        public static AuthorityValidator Empty => _default;
    }

    public class EmptyAuthorityValidator : AuthorityValidator
    {
        private static EmptyAuthorityValidator _default=new EmptyAuthorityValidator();
        
        public EmptyAuthorityValidator() : base(new string[0])
        {
        }

        public override bool IsContainsAll(params string[] codes)
        {
            return true;

            return base.IsContainsAll(codes);
        }

        public static EmptyAuthorityValidator Default => _default;
    }
}
