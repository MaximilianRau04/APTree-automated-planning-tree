import concretebt._parser.ConcreteBTParser;
import concretebt._ast.ASTWorld;
import concretebt._symboltable.ConcreteBTArtifactScope;
import concretebt._symboltable.IConcreteBTArtifactScope;
import concretebt._symboltable.ConcreteBTSymbols2Json;
import crftypedef._symboltable.ElementSymbol;
import crftypedef._symboltable.LocationSymbol;
import crftypedef._symboltable.AgentSymbol;
import concretebt._symboltable.BeamSymbol;
import concretebt._symboltable.PlateSymbol;
import concretebt._symboltable.FirstPositionSymbol;
import concretebt._symboltable.RobotSymbol;
import concretebt.ConcreteBTMill;
import de.se_rwth.commons.logging.Log;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Collection;
import java.util.Optional;

/**
 * InstanceSymbolsGenerator
 *
 * Generates one .sym file per concrete Element (e.g., Beam/Plate/Robot/FirstPosition)
 * from CRFConcreteInstances.bt so cross-file name resolution can autoload them.
 *
 * Usage:
 *   - args[0] = input .bt path (default: src/test/resources/valid/CRFConcrete/CRFConcreteInstances.bt)
 *   - args[1] = output dir for .sym files (default: target/symbols)
 */
public class InstanceSymbolsGenerator {

    private static final String DEFAULT_INPUT = "src/test/resources/valid/CRFConcrete/CRFConcreteInstances.bt";
    private static final String DEFAULT_OUTDIR = "target/symbols";

    public static void main(String[] args) {
        String input = args.length > 0 ? args[0] : DEFAULT_INPUT;
        String outDir = args.length > 1 ? args[1] : DEFAULT_OUTDIR;

        try {
            System.out.println("=== INSTANCE SYMBOLS GENERATOR ===");
            System.out.println("Input model: " + input);
            System.out.println("Output dir:  " + outDir);

            ConcreteBTMill.init();

            // Ensure output directory exists
            Path outPath = Paths.get(outDir);
            Files.createDirectories(outPath);

            // Parse instances model
            ASTWorld world = parseWorld(input);
            if (world == null) {
                System.err.println("✗ Failed to parse instances model. Abort.");
                return;
            }

            // Build initial artifact scope from AST
            IConcreteBTArtifactScope initial = ConcreteBTMill.scopesGenitorDelegator().createFromAST(world);

            int count = 0;

            // Beam/Plate (Element)
            for (BeamSymbol beam : initial.getBeamSymbols().values()) {
                count += writeSym(outPath, beam.getName(), beam);
            }
            for (PlateSymbol plate : initial.getPlateSymbols().values()) {
                count += writeSym(outPath, plate.getName(), plate);
            }

            // FirstPosition (Location)
            for (FirstPositionSymbol fp : initial.getFirstPositionSymbols().values()) {
                count += writeSym(outPath, fp.getName(), fp);
            }

            // Robot (Agent)
            for (RobotSymbol robot : initial.getRobotSymbols().values()) {
                count += writeSym(outPath, robot.getName(), robot);
            }

            if (count == 0) {
                System.out.println("⚠ No instance symbols found to serialize.");
            }

            System.out.println("✓ Wrote " + count + " symbol file(s) to: " + outPath.toAbsolutePath());

        } catch (Exception e) {
            System.err.println("✗ ERROR: " + e.getMessage());
            e.printStackTrace();
        }
    }

    private static ASTWorld parseWorld(String modelPath) throws IOException {
        File modelFile = new File(modelPath);
        if (!modelFile.exists()) {
            System.err.println("✗ Model not found: " + modelPath);
            System.err.println("  CWD: " + System.getProperty("user.dir"));
            return null;
        }
        ConcreteBTParser parser = new ConcreteBTParser();
        Optional<ASTWorld> res = parser.parse(modelPath);
        if (res.isEmpty()) {
            Log.getFindings().forEach(f -> System.err.println("  " + f.buildMsg()));
            return null;
        }
        return res.get();
    }

    private static int writeSym(Path outDir, String name, Object symbol) throws IOException {
        if (name == null || name.isBlank() || symbol == null) {
            return 0;
        }

        ConcreteBTArtifactScope single = new ConcreteBTArtifactScope();
        single.setName(name); // ensures loader searches <name>.sym

        // Add general symbols so general resolvers can find them.
        if (symbol instanceof ElementSymbol) {
            single.add((ElementSymbol) symbol);
        }
        if (symbol instanceof LocationSymbol) {
            single.add((LocationSymbol) symbol);
        }
        if (symbol instanceof AgentSymbol) {
            single.add((AgentSymbol) symbol);
        }

        // Also add the concrete symbols so concrete resolvers can find them.
        if (symbol instanceof BeamSymbol) {
            single.add((BeamSymbol) symbol);
        } else if (symbol instanceof PlateSymbol) {
            single.add((PlateSymbol) symbol);
        } else if (symbol instanceof FirstPositionSymbol) {
            single.add((FirstPositionSymbol) symbol);
        } else if (symbol instanceof RobotSymbol) {
            single.add((RobotSymbol) symbol);
        }

        String json = new ConcreteBTSymbols2Json().serialize(single);
        Path symPath = outDir.resolve(name + ".sym");
        try (FileWriter fw = new FileWriter(symPath.toFile())) {
            fw.write(json);
        }
        return 1;
    }
}
