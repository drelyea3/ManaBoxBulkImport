namespace ManaBoxBulkImport;

public static class InputParser
{
    private enum State
    {
        GettingCode,
        GotCode,
        GettingCount,
    }

    public static bool Parse(string input, out string? set, out string code, out bool isJapanese, out bool isFoil, out int count)
    {
        // isJapanese = j
        // isFoil = f
        // count = <somethig else><N>

        var state = State.GettingCode;

        set = null;
        code = string.Empty;
        isJapanese = false;
        isFoil = false;
        count = 1;

        int index = 0;

        while (index < input.Length)
        {
            var c = input[index];

            if (state == State.GettingCode)
            {
                switch (c)
                {
                    case ':':
                        if (code.Length == 0 || set?.Length > 0)
                        {
                            return false;
                        }

                        set = code;
                        code = string.Empty;
                        ++index;
                        continue;

                    case 'f' or 'F':
                        if (code.Length > 0)
                        {
                            isFoil = true;
                            state = State.GotCode;
                            ++index;
                            continue;
                        }
                        break;

                    case 'j' or 'J':
                        if (code.Length > 0)
                        {
                            isJapanese = true;
                            state = State.GotCode;
                            ++index;
                            continue;
                        }
                        break;

                    case ' ' or ',':
                        state = State.GotCode;
                        ++index;
                        continue;
                }

                code += c;
                ++index;
                continue;
            }
            else if (state == State.GotCode)
            {
                switch (c)
                {
                    case 'f' or 'F':
                        isFoil = true;
                        state = State.GotCode;
                        ++index;
                        continue;

                    case 'j' or 'J':
                        isJapanese = true;
                        state = State.GotCode;
                        ++index;
                        continue;

                    case ' ' or ',':
                        ++index;
                        continue;
                }

                if (char.IsDigit(c))
                {
                    count = c - '0';
                    state = State.GettingCount;
                    ++index;
                    continue;
                }
                else
                {
                    return false;
                }
            }
            else if (state == State.GettingCount)
            {
                if (char.IsDigit(c))
                {
                    count = (count * 10) + c - '0';
                    ++index;
                    continue;
                }

                state = State.GotCode;
            }
        }

        return code.Length > 0;
    }
}