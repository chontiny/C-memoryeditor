namespace Ana.Source.ScriptEngine.Memory
{
    using System;

    /// <summary>
    /// Interface to provide access to memory manipulations in an external process.
    /// </summary>
    public interface IMemoryCore
    {
        /// <summary>
        /// Returns the address of the specified module name. Returns 0 on failure.
        /// </summary>
        /// <param name="moduleName">The name of the module to calculate the address of.</param>
        /// <returns>The read address.</returns>
        UInt64 GetModuleAddress(String moduleName);

        /// <summary>
        /// Determines the size of the first instruction at the given address..
        /// </summary>
        /// <param name="assembly">The assembly code to measure.</param>
        /// <param name="address">The base address of the assembly code.</param>
        /// <returns>The size, in number of bytes, of the assembly code.</returns>
        Int32 GetAssemblySize(String assembly, UInt64 address);

        /// <summary>
        /// Converts the instruction with a frame of reference at a specific address to raw bytes.
        /// </summary>
        /// <param name="assembly">The assembly code to disassemble.</param>
        /// <param name="address">The base address of the assembly code.</param>
        /// <returns>The disassembled bytes of the assembly code.</returns>
        Byte[] GetAssemblyBytes(String assembly, UInt64 address);

        /// <summary>
        /// Returns the bytes of multiple instructions, as long as they are greater than the specified minimum.
        /// ie with a minimum of 16 bytes, if we get three 5 byte instructions, we will need to read the entire
        /// next instruction.
        /// </summary>
        /// <param name="address">The base address to begin reading instructions.</param>
        /// <param name="minimumInstructionBytes">The minimum number of bytes the instructions must take.</param>
        /// <returns>The bytes read from memory.</returns>
        Byte[] GetInstructionBytes(UInt64 address, Int32 minimumInstructionBytes);

        /// <summary>
        /// Allocates memory in the target process, and returns the address of the new memory.
        /// </summary>
        /// <param name="size">The size of the allocation.</param>
        /// <returns>The address of the allocated memory.</returns>
        UInt64 AllocateMemory(Int32 size);

        /// <summary>
        /// Deallocates memory previously allocated at a specified address.
        /// </summary>
        /// <param name="address">The address to perform the deallocation.</param>
        void DeallocateMemory(UInt64 address);

        /// <summary>
        /// Deallocates all allocated memory for the parent script.
        /// </summary>
        void DeallocateAllMemory();

        /// <summary>
        /// Creates a code cave that jumps from a given entry address and executes the given assembly.
        /// </summary>
        /// <param name="entry">The address to jump to the code cave.</param>
        /// <param name="assembly">The assembly code to disassemble and inject into the code cave.</param>
        /// <returns>The address of the code cave.</returns>
        UInt64 CreateCodeCave(UInt64 entry, String assembly);

        /// <summary>
        /// Determines the address that a code cave would need to return to, if one were to be created at the specified address..
        /// </summary>
        /// <param name="address">The address of the code cave.</param>
        /// <returns>The address to which the code cave will return upon completion.</returns>
        UInt64 GetCaveExitAddress(UInt64 address);

        /// <summary>
        /// Removes a created code cave at the specified address..
        /// </summary>
        /// <param name="address">The address of the code cave.</param>
        void RemoveCodeCave(UInt64 address);

        /// <summary>
        /// Removes all created code caves by the parent script.
        /// </summary>
        void RemoveAllCodeCaves();

        /// <summary>
        /// Binds a keyword to a given value for use in the script.
        /// </summary>
        /// <param name="keyword">The local keyword to bind.</param>
        /// <param name="address">The address to which the keyword is bound.</param>
        void SetKeyword(String keyword, UInt64 address);

        /// <summary>
        /// Binds a keyword to a given value for use in all scripts.
        /// </summary>
        /// <param name="globalKeyword">The global keyword to bind.</param>
        /// <param name="address">The address to which the keyword is bound.</param>
        void SetGlobalKeyword(String globalKeyword, UInt64 address);

        /// <summary>
        /// Clears the specified keyword created by the parent script.
        /// </summary>
        /// <param name="keyword">The local keyword to clear.</param>
        void ClearKeyword(String keyword);

        /// <summary>
        /// Clears the specified global keyword created by any script.
        /// </summary>
        /// <param name="globalKeyword">The global keyword to clear.</param>
        void ClearGlobalKeyword(String globalKeyword);

        /// <summary>
        /// Clears all keywords created by the parent script.
        /// </summary>
        void ClearAllKeywords();

        /// <summary>
        /// Clears all global keywords created by any script.
        /// </summary>
        void ClearAllGlobalKeywords();

        /// <summary>
        /// Searches for the first address that matches the array of bytes.
        /// </summary>
        /// <param name="bytes">The array of bytes to search for.</param>
        /// <returns>The address of the first first array of byte match.</returns>
        UInt64 SearchAOB(Byte[] bytes);

        /// <summary>
        /// Searches for the first address that matches the given array of byte pattern.
        /// </summary>
        /// <param name="pattern">The pattern string for which to search.</param>
        /// <returns>The address of the first first pattern match.</returns>
        UInt64 SearchAOB(String pattern);

        /// <summary>
        /// Searches for all addresses that match the given array of byte pattern.
        /// </summary>
        /// <param name="pattern">The array of bytes to search for.</param>
        /// <returns>The addresses of all matches.</returns>
        UInt64[] SearchAllAob(String pattern);

        /// <summary>
        /// Reads the Double at the given address.
        /// </summary>
        /// <param name="address">The address of the read.</param>
        /// <returns>The Double read from memory.</returns>
        T ReadMemory<T>(UInt64 address);

        /// <summary>
        /// Reads the array of bytes of the specified count at the given address.
        /// </summary>
        /// <param name="address">The address of the read.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The bytes read at the address.</returns>
        Byte[] ReadMemory(UInt64 address, Int32 count);

        /// <summary>
        /// Writes the value at the specified address.
        /// </summary>
        /// <param name="address">The address of the write.</param>
        /// <param name="T">The value of the write.</param>
        void WriteMemory<T>(UInt64 address, T value);

        /// <summary>
        /// Writes the Byte array to the specified address
        /// </summary>
        /// <param name="address">The address of the write.</param>
        /// <param name="values">The values of the write.</param>
        void WriteMemory(UInt64 address, Byte[] values);
    }
    //// End interface
}
//// End namespace