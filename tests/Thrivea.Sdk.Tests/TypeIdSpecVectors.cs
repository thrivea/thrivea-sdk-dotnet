// Generated from the TypeID specification's test vectors (https://github.com/jetify-com/typeid/tree/main/spec).

namespace Thrivea.Sdk.Tests;

public static class TypeIdSpecVectors
{
    public static IEnumerable<object[]> Valid =>
    [
        ["nil", "00000000000000000000000000", "", "00000000-0000-0000-0000-000000000000"],
        ["one", "00000000000000000000000001", "", "00000000-0000-0000-0000-000000000001"],
        ["ten", "0000000000000000000000000a", "", "00000000-0000-0000-0000-00000000000a"],
        ["sixteen", "0000000000000000000000000g", "", "00000000-0000-0000-0000-000000000010"],
        ["thirty-two", "00000000000000000000000010", "", "00000000-0000-0000-0000-000000000020"],
        ["max-valid", "7zzzzzzzzzzzzzzzzzzzzzzzzz", "", "ffffffff-ffff-ffff-ffff-ffffffffffff"],
        ["valid-alphabet", "prefix_0123456789abcdefghjkmnpqrs", "prefix", "0110c853-1d09-52d8-d73e-1194e95b5f19"],
        ["valid-uuidv7", "prefix_01h455vb4pex5vsknk084sn02q", "prefix", "01890a5d-ac96-774b-bcce-b302099a8057"],
        ["prefix-underscore", "pre_fix_00000000000000000000000000", "pre_fix", "00000000-0000-0000-0000-000000000000"],
    ];

    public static IEnumerable<object[]> Invalid =>
    [
        ["prefix-uppercase", "PREFIX_00000000000000000000000000", "The prefix should be lowercase with no uppercase letters"],
        ["prefix-numeric", "12345_00000000000000000000000000", "The prefix can't have numbers, it needs to be alphabetic"],
        ["prefix-period", "pre.fix_00000000000000000000000000", "The prefix can't have symbols, it needs to be alphabetic"],
        ["prefix-non-ascii", "préfix_00000000000000000000000000", "The prefix can only have ascii letters"],
        ["prefix-spaces", "  prefix_00000000000000000000000000", "The prefix can't have any spaces"],
        ["prefix-64-chars", "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijkl_00000000000000000000000000", "The prefix can't be 64 characters, it needs to be 63 characters or less"],
        ["separator-empty-prefix", "_00000000000000000000000000", "If the prefix is empty, the separator should not be there"],
        ["separator-empty", "_", "A separator by itself should not be treated as the empty string"],
        ["suffix-short", "prefix_1234567890123456789012345", "The suffix can't be 25 characters, it needs to be exactly 26 characters"],
        ["suffix-long", "prefix_123456789012345678901234567", "The suffix can't be 27 characters, it needs to be exactly 26 characters"],
        ["suffix-spaces", "prefix_1234567890123456789012345 ", "The suffix can't have any spaces"],
        ["suffix-uppercase", "prefix_0123456789ABCDEFGHJKMNPQRS", "The suffix should be lowercase with no uppercase letters"],
        ["suffix-hyphens", "prefix_123456789-123456789-123456", "The suffix can't have any hyphens"],
        ["suffix-wrong-alphabet", "prefix_ooooooiiiiiiuuuuuuulllllll", "The suffix should only have letters from the spec's alphabet"],
        ["suffix-ambiguous-crockford", "prefix_i23456789ol23456789oi23456", "The suffix should not have any ambiguous characters from the crockford encoding"],
        ["suffix-hyphens-crockford", "prefix_123456789-0123456789-0123456", "The suffix can't ignore hyphens as in the crockford encoding"],
        ["suffix-overflow", "prefix_8zzzzzzzzzzzzzzzzzzzzzzzzz", "The suffix should encode at most 128-bits"],
        ["prefix-underscore-start", "_prefix_00000000000000000000000000", "The prefix can't start with an underscore"],
        ["prefix-underscore-end", "prefix__00000000000000000000000000", "The prefix can't end with an underscore"],
        ["empty", "", "The empty string is not a valid typeid"],
        ["prefix-empty", "prefix_", "The suffix can't be the empty string"],
    ];
}
