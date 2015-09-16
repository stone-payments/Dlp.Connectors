# Dlp.Framework
Common framework containing utilities and extension methods for fast development.

The Dlp.Framework package contains a serie of utilities and extension methods that are used by almost major softwares in it's development process.

The following funcionality are available in this release:

## Extension methods:

  - ByteExtensions
    - GetString(): Converts a byte array to an string.
  
  - CollectionExtensions
    - AsString(): Converts a collection into a string. You can choose the separator and any surrounding character for each element.
  
  - DateTimeExtensions
    - ChangeTimeZone(): Converts a DateTime object to the specified TimeZone. Daylight Saving Time are calculated automatically.
    - SystemTimeZones(): Get all the available timezones from system.
    - ToIso8601String(): Converts the DateTime objeto to an ISO8601 string.
  
  - ObjectExtensions
    - Clone(): Creates a new instance of the current object without references to the original.
    - DiffProperties(): Compares the current object with another one. The divergent properties values are detected and returned.
  
  - StringExtensions
    - AsCnpj(): Formats a string as CNPJ (00.000.000/0001-00)
    - AsCpf(): Formats a string as CPF (999.000.000-99)
    - AsPhoneNumber: Formats a string as a phone number.
    - AsZipCode: Formats a string as a ZipCode.
    - CalculateMd5(): Calculates the MD5 for a string.
    - CalculateSha1(): Calculates ths hash from a string, using the SHA1 algorithm.
    - CalculateSha256(): Calculates ths hash from a string, using the SHA256 algorithm.
    - CalculateSha384(): Calculates ths hash from a string, using the SHA384 algorithm.
    - CalculateSha512(): Calculates ths hash from a string, using the SHA512 algorithm.
    - Decrypt(): Decrypts a string using the provided secretKey.
    - Encrypt(): Encrypts a string using the provided secretKey.
    - GetBytes(): Converts a string to a byte array.
    - GetDigits(): Gets all the digits of the specified string.
    - GetLetters(): Gets all the letters of the specified string.
    - GetLettersOrDigits(): Gets all the letters and digits of the specified string.
    - IsValidCnpj(): Checks if the current CNPJ is valid.
    - IsValidCpf(): Checks if the current CPF is valid.
    - Mask(): Masks the content of the specified string.
    - RemoveAccentuation(): Replaces all the accented characters with its unaccented version.

## Utility classes:

  - MailService
    - SendEmail(): Sends an email.
    - SendEmailAsync(): Sends an email asynchronously.

  - RestClient
    - SendHttpWebRequest(): Sends an HTTP request to the specified endpoint.
    - SendHttpWebRequestAsync(): Sends an HTTP request to the specified endpoint asynchronously.

  - Serializer
    - BinaryDeserialize(): Deserializes a byte array to a new instance of the specified type.
    - BinarySerialize(): Serializes the specified object to a byte array.
    - JsonDeserialize(): Deserializes a JSON string to a new instance of the specified type.
    - JsonSerialize(): Serializes an object to a JSON string.
    - XmlDeserialize(): Deserializes a XML string to a new instance of the specified type.
    - XmlSerialize(): Serializes an object to a XML string.

## Extended funcionality:

  - IocFactory
    The IocFactory is a simplified Dependency Injection Container. The most common features that are present on most well know containers are present in this version of the framework, without the need to configure or install third party dependencies.
    
    The IocFactory funcionality can be found in the Dlp.Framework.Container namespace.
    
  - Mocker
    The framework contains a mocker class that is intended to make the test process easier without the need to install third party dependencies.
    
    The IocFactory funcionality can be found in the Dlp.Framework.Mock namespace.

## Install from nuget.org

The official version can be obtained from the nuget package manager with the following command line:

**PM> Install-Package Dlp.Framework.dll**
