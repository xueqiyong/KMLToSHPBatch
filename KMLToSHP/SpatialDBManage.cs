using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMLToSHP
{
    class SpatialDBManage
    {
        public static IFeatureClass GetFileGeodatabase(string gdbFile, string featureName)
        {
            IFeatureWorkspace featureWorkspace;
            IFeatureClass featureClass;

            FileGDBWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
            featureWorkspace = workspaceFactory.OpenFromFile(gdbFile, 0) as IFeatureWorkspace;
            featureClass = featureWorkspace.OpenFeatureClass(featureName);

            return featureClass;
        }

        public static void ConvertFeatureClassToShapefile(string sourceWorkspacePath, string targetWorkspacePath, string sourceDataName, string targetDataName)
        {
          // Open the source and target workspaces.
          //String sourceWorkspacePath = @"C:\arcgis\DeveloperKit\SamplesNET\data\Atlanta.gdb";
          //String targetWorkspacePath = @"C:\Temp";
          IWorkspaceFactory sourceWorkspaceFactory = new FileGDBWorkspaceFactoryClass();
          IWorkspaceFactory targetWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
          IWorkspace sourceWorkspace = sourceWorkspaceFactory.OpenFromFile
            (sourceWorkspacePath, 0);
          IWorkspace targetWorkspace = targetWorkspaceFactory.OpenFromFile
            (targetWorkspacePath, 0);

          // Cast the workspaces to the IDataset interface and get name objects.
          IDataset sourceWorkspaceDataset = (IDataset)sourceWorkspace;
          IDataset targetWorkspaceDataset = (IDataset)targetWorkspace;
          IName sourceWorkspaceDatasetName = sourceWorkspaceDataset.FullName;
          IName targetWorkspaceDatasetName = targetWorkspaceDataset.FullName;
          IWorkspaceName sourceWorkspaceName = (IWorkspaceName)
            sourceWorkspaceDatasetName;
          IWorkspaceName targetWorkspaceName = (IWorkspaceName)
            targetWorkspaceDatasetName;

          // Create a name object for the shapefile and cast it to the IDatasetName interface.
          IFeatureClassName sourceFeatureClassName = new FeatureClassNameClass();
          IDatasetName sourceDatasetName = (IDatasetName)sourceFeatureClassName;
          sourceDatasetName.Name = sourceDataName;//"streets";
          sourceDatasetName.WorkspaceName = sourceWorkspaceName;

          // Create a name object for the FGDB feature class and cast it to the IDatasetName interface.
          IFeatureClassName targetFeatureClassName = new FeatureClassNameClass();
          IDatasetName targetDatasetName = (IDatasetName)targetFeatureClassName;
          targetDatasetName.Name = targetDataName;// "AtlantaStreets";
          targetDatasetName.WorkspaceName = targetWorkspaceName;

          // Open source feature class to get field definitions.
          IName sourceName = (IName)sourceFeatureClassName;
          IFeatureClass sourceFeatureClass = (IFeatureClass)sourceName.Open();

          // Create the objects and references necessary for field validation.
          IFieldChecker fieldChecker = new FieldCheckerClass();
          IFields sourceFields = sourceFeatureClass.Fields;
          IFields targetFields = null;
          IEnumFieldError enumFieldError = null;

          // Set the required properties for the IFieldChecker interface.
          fieldChecker.InputWorkspace = sourceWorkspace;
          fieldChecker.ValidateWorkspace = targetWorkspace;

          // Validate the fields and check for errors.
          fieldChecker.Validate(sourceFields, out enumFieldError, out targetFields);
          if (enumFieldError != null)
          {
            // Handle the errors in a way appropriate to your application.
            Console.WriteLine("Errors were encountered during field validation.");
          }

          // Find the shape field.
          String shapeFieldName = sourceFeatureClass.ShapeFieldName;
          int shapeFieldIndex = sourceFeatureClass.FindField(shapeFieldName);
          IField shapeField = sourceFields.get_Field(shapeFieldIndex);

          // Get the geometry definition from the shape field and clone it.
          IGeometryDef geometryDef = shapeField.GeometryDef;
          IClone geometryDefClone = (IClone)geometryDef;
          IClone targetGeometryDefClone = geometryDefClone.Clone();
          IGeometryDef targetGeometryDef = (IGeometryDef)targetGeometryDefClone;

          // Create a query filter to remove ramps, interstates and highways.
          IQueryFilter queryFilter = new QueryFilterClass();
          //queryFilter.WhereClause = "NAME <> 'Ramp' AND PRE_TYPE NOT IN ('I', 'Hwy')";
          queryFilter.WhereClause = "1=1";
          // Create the converter and run the conversion.
          IFeatureDataConverter featureDataConverter = new FeatureDataConverterClass();
          IEnumInvalidObject enumInvalidObject =
            featureDataConverter.ConvertFeatureClass(sourceFeatureClassName,
            queryFilter, null, targetFeatureClassName, targetGeometryDef, targetFields,
            "", 1000, 0);

          // Check for errors.
          IInvalidObjectInfo invalidObjectInfo = null;
          enumInvalidObject.Reset();
          while ((invalidObjectInfo = enumInvalidObject.Next()) != null)
          {
            // Handle the errors in a way appropriate to the application.
            Console.WriteLine("Errors occurred for the following feature: {0}",
              invalidObjectInfo.InvalidObjectID);
          }
        }

    }
}
