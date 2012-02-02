/* ************************************************************************ *
 To the extent possible under law, Nils Maier has waived all copyright and
 related or neighboring rights to string_natural_compare.
 http://creativecommons.org/publicdomain/zero/1.0/
 * ************************************************************************ */

#ifndef __NATCAMP_H__
#define __NATCAMP_H__

#include <ctype.h>
#include <stdlib.h>

namespace string_natural_compare_ns {
  template<typename char_type, class func_tol>
  int impl(const char_type *s1, const char_type *s2, func_tol ToLong)
  {
    for (
      register char_type c1, c2, *ep1, *ep2;
      (c1 = *s1) && (c2 = *s2);
      ++s1, ++s2
    ) {
      // Get leading numbers
      // Note: the CRT tol implementation will remove leading white space
      // This is ok for our purpose
      const long n1 = ToLong(s1, &ep1, 10),
        n2 = ToLong(s2, &ep2, 10);
      // Got numbers?
      const bool isn1 = n1 || s1 != ep1,
        isn2 = n2 || s2 != ep2;

      // Both not a number; regular character compare
      if (!isn1 && !isn2) {
        const int diff = (int)c1 - c2;
        if (diff) {
          return diff;
        }
        continue;
      }

      // Both are numbers
      if (isn1 && isn2) {
        if (n1 > n2) {
          return 1;
        }
        if (n2 > n1) {
          return -1;
        }

        // Same number, but might contain different front-padding
        // Larger front-padding is cmp-smaller
        const size_t d1 = ep1 - s1,
          d2 = ep2 - s2;
        if (d1 < d2) {
          return 1;
        }
        if (d2 < d1) {
          return -1;
        }

        // Same number
        s1 = ep1 - 1;
        s2 = ep2 - 1;
        continue;
      }

      // s1 contains a number, while s2 does not
      // hence s1 < s2
      if (isn1) {
        return -1;
      }

      // s2 contains a number, while s2 does not
      // hence s1 > s2
      if (isn2) {
        return 1;
      }
    }

    // len(s1) > len(s2)
    if (*s1) {
      return 1;
    }
    // len(s1) < len(s2)
    if (*s2) {
      return -1;
    }
    return 0;
  }

  // version with transformations
  template<typename char_type, class func_tol, class func_transform>
  int impl(const char_type *s1, const char_type *s2, func_tol ToLong, func_transform Transform)
  {
    // For comments see implementation without transformations
    for (
      register char_type c1, c2, *ep1, *ep2;
      (c1 = *s1) && (c2 = *s2);
      ++s1, ++s2
    ) {
      const long n1 = ToLong(s1, &ep1, 10),
        n2 = ToLong(s2, &ep2, 10);
      const bool isn1 = n1 || s1 != ep1,
        isn2 = n2 || s2 != ep2;
      if (!isn1 && !isn2) {
        const int diff = (int)Transform(c1) - Transform(c2);
        if (diff) {
          return diff;
        }
        continue;
      }
      if (isn1 && isn2) {
        if (n1 > n2) {
          return 1;
        }
        if (n2 > n1) {
          return -1;
        }
        const size_t d1 = ep1 - s1,
          d2 = ep2 - s2;
        if (d1 < d2) {
          return 1;
        }
        if (d2 < d1) {
          return -1;
        }
        s1 = ep1 - 1;
        s2 = ep2 - 1;
        continue;
      }
      if (isn1) {
        return -1;
      }
      if (isn2) {
        return 1;
      }
    }
    if (*s1) {
      return 1;
    }
    if (*s2) {
      return -1;
    }
    return 0;
  }
};

inline int string_natural_compare(const char* s1, const char *s2) {
  return string_natural_compare_ns::impl(s1, s2, ::strtol);
}
inline int string_natural_compare(const wchar_t* s1, const wchar_t *s2) {
  return string_natural_compare_ns::impl(s1, s2, ::wcstol);
}
inline int string_natural_compare_ic(const char* s1, const char *s2) {
  return string_natural_compare_ns::impl(s1, s2, ::strtol, ::tolower);
}
inline int string_natural_compare_ic(const wchar_t* s1, const wchar_t *s2) {
  return string_natural_compare_ns::impl(s1, s2, ::wcstol, ::towlower);
}

#endif