This project is separated from PIMS.Data in order to accomodate
separating NHibernate (OR/M) configuration and mapping
definitions from the domain. Contained herein will be the OR/M mappings/configurations
as well as data repository implementations.

Changes in a data provider should have no affect on the domain model as
defined in PIMS.Data.