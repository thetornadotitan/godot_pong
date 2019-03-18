using Godot;
using System;

public class Game : Node2D
{
    int leftScore = 0;
    int rightScore = 0;
    bool shooter = false;

    KinematicBody2D ball;
    VisibilityNotifier2D ballVisibilityChecker;
    float ballSpeed = 80f;
    float ballStartSpeed = 80f;

    Vector2 ballVel;
    Vector2 ballStartPos;
    Vector2 leftPaddleStartPos;
    Vector2 rightPaddleStartPos;

    KinematicBody2D leftPaddle;
    KinematicBody2D rightPaddle;
    float paddleSpeed = 150f;
    bool leftPaddleUp, leftPaddleDown, rightPaddleUp, rightPaddleDown = false;

    Random prng;

    bool play = false;

    RichTextLabel leftScoreLabel;
    RichTextLabel rightScoreLabel;
    RichTextLabel instructions;

    public override void _Ready()
    {
        prng = new Random();
        InitBall();
        InitPaddles();
        InitLables();
    }

    public override void _Process(float delta)
    {
        if (play)
        {
            if (Input.IsActionJustPressed("left_paddle_up"))
            {
                leftPaddleUp = true;
            }
            if (Input.IsActionJustReleased("left_paddle_up"))
            {
                leftPaddleUp = false;
            }

            if (Input.IsActionJustPressed("left_paddle_down"))
            {
                leftPaddleDown = true;
            }
            if (Input.IsActionJustReleased("left_paddle_down"))
            {
                leftPaddleDown = false;
            }
        }
        else
        {
            if (Input.IsActionJustReleased("start"))
            {
                play = true;
                instructions.Visible = false;
            }
        }

        if (!ballVisibilityChecker.IsOnScreen())
        {
            if (!shooter) leftScore++;
            if (shooter) rightScore++;
            updateScoreText();
            resetGame();
            play = false;
            instructions.Visible = true;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (play)
        {
            float leftMove = (leftPaddleUp) ? -paddleSpeed : (leftPaddleDown) ? paddleSpeed : 0;
            Vector2 rightMove = new Vector2(0, ball.Position.y - rightPaddle.Position.y);
            rightMove *= 10;

            if (Math.Abs(rightMove.y) > paddleSpeed)
            {
                int sign = Math.Sign(rightMove.y);
                if (sign == -1) rightMove.y = -paddleSpeed;
                else rightMove.y = paddleSpeed;
            }

            leftPaddle.MoveAndCollide(new Vector2(0, leftMove) * delta);
            rightPaddle.MoveAndCollide(rightMove * delta);

            KinematicCollision2D col = ball.MoveAndCollide(ballVel * delta);
            if (col != null)
            {
                if (col.Collider == leftPaddle)
                {
                    ballVel = ball.Position - leftPaddle.Position;
                    ballSpeed += 10f;
                    ballVel = ballVel.Normalized() * ballSpeed;
                    shooter = (Math.Sign(ballVel.x) == -1) ? true : false;
                }
                else if (col.Collider == rightPaddle)
                {
                    ballVel = ball.Position - rightPaddle.Position;
                    ballSpeed += 25f;
                    ballVel = ballVel.Normalized() * ballSpeed;
                    shooter = (Math.Sign(ballVel.x) == -1) ? true : false;
                }
                else
                {
                    GD.Print(ballVel);
                    ballVel = ballVel.Bounce(col.Normal);
                    ballVel = ballVel.Normalized() * ballSpeed;
                    GD.Print(ballVel);
                }
            }
        }
    }

    private void InitBall()
    {
        ball = GetNode(new NodePath("./Ball")) as KinematicBody2D;
        ballVisibilityChecker = ball.GetChild(2) as VisibilityNotifier2D;
        ballStartPos = ball.Position;

        RandomizeBallVelocity();

        shooter = (Math.Sign(ballVel.x) == -1) ? true : false;
    }

    private void InitPaddles()
    {
        leftPaddle = GetNode(new NodePath("./Paddle Left")) as KinematicBody2D;
        rightPaddle = GetNode(new NodePath("./Paddle Right")) as KinematicBody2D;
        leftPaddleStartPos = leftPaddle.Position;
        rightPaddleStartPos = rightPaddle.Position;
    }

    private void InitLables()
    {
        leftScoreLabel = GetNode(new NodePath("Left Score Text")) as RichTextLabel;
        rightScoreLabel = GetNode(new NodePath("Right Score Text")) as RichTextLabel;
        instructions = GetNode(new NodePath("Instructions")) as RichTextLabel;

        updateScoreText();
    }

    private void updateScoreText()
    {
        leftScoreLabel.BbcodeText = leftScore.ToString();
        rightScoreLabel.BbcodeText = "[right]" + rightScore.ToString() + "[/right]";
    }

    private void RandomizeBallVelocity()
    {
        ballVel = new Vector2(prng.Next(-100, 101), prng.Next(-50, 51));
        //ballVel = new Vector2(50f, 45f);
        if (ballVel.x < 5 && ballVel.x > -5)
        {
            if (prng.Next(0, 2) == 0)
            {
                ballVel.x = 5;
            }
            else
            {
                ballVel.x = -5;
            }
        }
        ballVel = ballVel.Normalized() * ballSpeed;
    }

    private void resetGame()
    {
        ball.Position = ballStartPos;
        leftPaddle.Position = leftPaddleStartPos;
        rightPaddle.Position = rightPaddleStartPos;
        ballSpeed = ballStartSpeed;
        RandomizeBallVelocity();
    }
}
