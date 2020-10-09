import static java.lang.System.out;

public class Program {


    public static void main(String[] args)
    {
        out.println("Hello World!");


    }

    // Sleeper function
    public static void sleep()
    {
        Integer ms = 1000;
        try
        {
            Thread.sleep(ms);
        }
        catch(InterruptedException ex)
        {
            Thread.currentThread().interrupt();
        }
    }
}
