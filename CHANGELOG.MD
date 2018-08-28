# dotEntity Change Log

## 1.1.0
- Added support for enumeration types
- Added support for GUID
- Allowed nullable string types
- Quoted query objects to avoid keyword conflicts
- Added support to ignore some properties of entities for query generation
- SELECT query generation will use explicit column names rather than wildcards (better for performance)
- Used 'Id' as default primary key column name
- Bug Fixes
- Performance refactorings

## 1.0.0 (Stable) - 2017-08-09
- Query Caching added to improve performance
- Ordinal Caching added to improve performance
- Command behavior added to query processing
- Exception handling refined to improve performance

## 0.2.0-rc1 - 2017-07-27
- Bugs Fixed
- Sqlite Provider Implemented
- Added nullable getters and setters
- Paginate queries only if required
- Fixed unit tests and streamlined tests

## 0.1.0-rc1 - 2017-07-25
- First Release - Hello World :)
