import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.BeforeEach;
import static org.junit.jupiter.api.Assertions.*;
import java.io.File;
import java.nio.file.Files;
import java.nio.file.Paths;

public class DynamicBTFlowNodeParserTest {
    
    @BeforeEach
    void setUp() {
        // Setup will be added when parser is generated
    }
    
    @Test
    void testFlowNodeFileExists() {
        try {
            System.out.println("Testing DynamicBTFlowNode test file existence...");
            
            File testFile = new File("src/test/resources/valid/crf/test_FlowNode.txt");
            assertTrue(testFile.exists(), "Test file should exist");
            
            String content = Files.readString(Paths.get(testFile.getPath()));
            assertNotNull(content, "Test file should not be empty");
            assertTrue(content.length() > 0, "Test file should have content");
            
            System.out.println("SUCCESS: DynamicBTFlowNode test file is valid!");
            System.out.println("File content length: " + content.length() + " characters");
            
        } catch (Exception e) {
            System.err.println("ERROR checking test file: " + e.getMessage());
            e.printStackTrace();
            fail("Test file check failed: " + e.getMessage());
        }
    }
    
    @Test
    void testAlternativeFlowNodeFileExists() {
        try {
            System.out.println("Testing Alternative DynamicBTFlowNode test file existence...");
            
            File testFile = new File("src/test/resources/valid/crf/test_FlowNode_Alternative.txt");
            assertTrue(testFile.exists(), "Alternative test file should exist");
            
            String content = Files.readString(Paths.get(testFile.getPath()));
            assertNotNull(content, "Alternative test file should not be empty");
            assertTrue(content.length() > 0, "Alternative test file should have content");
            
            System.out.println("SUCCESS: Alternative DynamicBTFlowNode test file is valid!");
            
        } catch (Exception e) {
            System.err.println("ERROR checking alternative test file: " + e.getMessage());
            e.printStackTrace();
            fail("Alternative test file check failed: " + e.getMessage());
        }
    }
    
    @Test
    void testSimpleFlowNodeFileExists() {
        try {
            System.out.println("Testing Simple DynamicBTFlowNode test file existence...");
            
            File testFile = new File("src/test/resources/valid/crf/test_FlowNode_Simple.txt");
            assertTrue(testFile.exists(), "Simple test file should exist");
            
            String content = Files.readString(Paths.get(testFile.getPath()));
            assertNotNull(content, "Simple test file should not be empty");
            assertTrue(content.length() > 0, "Simple test file should have content");
            
            System.out.println("SUCCESS: Simple DynamicBTFlowNode test file is valid!");
            
        } catch (Exception e) {
            System.err.println("ERROR checking simple test file: " + e.getMessage());
            e.printStackTrace();
            fail("Simple test file check failed: " + e.getMessage());
        }
    }
    
    @Test
    void testGrammarFileExists() {
        try {
            System.out.println("Testing DynamicBTFlowNode grammar file existence...");
            
            File grammarFile = new File("src/main/grammars/DynamicBTFlowNode.mc4");
            assertTrue(grammarFile.exists(), "Grammar file should exist");
            
            String content = Files.readString(Paths.get(grammarFile.getPath()));
            assertNotNull(content, "Grammar file should not be empty");
            assertTrue(content.length() > 0, "Grammar file should have content");
            
            System.out.println("SUCCESS: DynamicBTFlowNode grammar file is valid!");
            System.out.println("Grammar content length: " + content.length() + " characters");
            
        } catch (Exception e) {
            System.err.println("ERROR checking grammar file: " + e.getMessage());
            e.printStackTrace();
            fail("Grammar file check failed: " + e.getMessage());
        }
    }
}
