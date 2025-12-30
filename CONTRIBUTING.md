# Contributing to PCG RNG for .NET

Thank you for your interest in contributing to this .NET port of the PCG random number generator!

## How to Contribute

### Reporting Issues

- Check if the issue already exists before creating a new one
- Provide clear steps to reproduce the problem
- Include relevant information (OS, .NET version, error messages)
- For security issues, please report privately

### Submitting Changes

1. **Fork the repository** and create a new branch from `main`
2. **Make your changes** with clear, focused commits
3. **Write tests** for new functionality or bug fixes
4. **Run the test suite** to ensure nothing breaks:
   ```bash
   cd dotnet
   dotnet test
   ```
5. **Update documentation** if you're changing behavior or adding features
6. **Submit a pull request** with a clear description of your changes

### Code Style

- Follow existing code conventions in the project
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise

### Commit Messages

- Use clear, descriptive commit messages
- Start with a verb in the imperative mood (e.g., "Add", "Fix", "Update")
- Use conventional commit prefixes when applicable:
  - `feat:` for new features
  - `fix:` for bug fixes
  - `docs:` for documentation changes
  - `test:` for test additions or changes
  - `perf:` for performance improvements
  - `refactor:` for code refactoring
  - `build:` for build system changes

### Testing

- All new code should include appropriate tests
- Oracle tests validate against the reference C++ implementation
- Unit tests cover edge cases and API behavior
- Run `dotnet test` before submitting

### Oracle Testing

This project uses oracle tests that compare output against the official C++ PCG implementation. If you modify generator algorithms:

1. Update the C++ oracle generator in `dotnet/tools/OracleGenerator/`
2. Regenerate oracle data files
3. Ensure all oracle tests pass

## License

By contributing to this project, you agree that your contributions will be licensed under the same dual license (Apache 2.0 OR MIT) as the rest of the project.

## Questions?

Feel free to open an issue for any questions about contributing!
